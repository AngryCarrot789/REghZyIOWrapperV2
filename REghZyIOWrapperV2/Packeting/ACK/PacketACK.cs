using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using REghZyIOWrapperV2.Packeting.Exceptions;
using REghZyIOWrapperV2.Streams;
using static REghZyIOWrapperV2.Packeting.ACK.PacketACK;

namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// ACK Header = 4 bytes (Destination and idempotency key bit shifted together)
    /// The high 29 bits are the ACK IDs (0 to 536870911 (536 million!))
    /// The low 3 bits are the destination codes (0-7)
    /// 
    /// <para>
    /// The 2 generals problem is about acknowledgement 
    /// 'A' sends a message to 'B', but how does 'A' know 'B' has received the message?
    /// 'B' could send a message to 'A' but 'A' may not receive it due to packet loss maybe
    /// So 'B' cannot know for certain that 'A' has received a message,
    /// and back at the start, 'A' cannot really know 'B' has received their message.
    /// This problem is impossible to 100% solve, but there are solutions that work most of the time,
    /// such as what i wrote below:
    /// </para>
    /// 
    /// <para>
    /// When stuff like banking is done, you use a token, which you generate for each transation.
    /// That way, you can send 1000s of the EXACT SAME PACKET, and as long as the server receives just 1 of them,
    /// it will process that one transaction and send a (or many) packet back.
    /// And because you sent 1000, it will dump any other packets it receives with the same token,
    /// ignoring it, so that it doesn't keep repeating the same transaction
    /// This token is also known as an idempotency token (or idempotency key)
    /// </para>
    /// <para>
    /// See this video where tom scott explains it very well: https://www.youtube.com/watch?v=IP-rGJKSZ3s
    /// (tom got me into making this idempotency stuff too, lol)
    /// </para>
    /// </summary>
    public abstract class PacketACK : Packet {
        // The size of an ACK packet's header (not including the base packet header size)
        protected const uint ACK_HEADER_SIZE = 4;

        // Including the base packet's header, the total ACK packet header size is 7 bytes
        // These 2 constants aren't used... they are just for me to remember the sizes ;)
        protected const uint ACK_TOTAL_HEADER = HEADER_SIZE + ACK_HEADER_SIZE;

        /// <summary>
        /// The minimum allowed idempotency key. This is 1 due to how the <see cref="IdempotencyKeyStore"/> works
        /// </summary>
        public const uint IDEMPOTENCY_MIN = 1;

        // This is the maximum value you can store in 29 bits of data
        // and it should be enough to support a huge amount of ACK packets
        // for a very long runtime.
        // even if you send 5000 ACK packets every second, this value will
        // still be able to handle that many for atleast 29.8 hours.
        // And if you go for a more realistic amount of packets, e.g 10 per second,
        // it will last 621 days! Plenty!!
        // 00011111_11111111_11111111_11111111
        public const uint IDEMPOTENCY_MAX = 536_870_911;

        public delegate TPacket PacketACKCreator<out TPacket>(DestinationCode dest, uint key, IDataInput input, ushort length) where TPacket : PacketACK;

        /// <summary>
        /// A dictionary that maps the packet type to it's next available ID. If there isn't an entry for a
        /// packet, it is added (starting at <see cref="IDEMPOTENCY_MIN"/>)
        /// </summary>
        private static readonly Dictionary<Type, uint> TypeToNextID;
        private static readonly Dictionary<Type, IdempotencyKeyStore> ServerTypeToIKS;
        private static readonly Dictionary<Type, IdempotencyKeyStore> ClientTypeToIKS;
        private static readonly PacketACKCreator<PacketACK>[] CreateFromClient;
        private static readonly PacketACKCreator<PacketACK>[] CreateFromServer;

        static PacketACK() {
            TypeToNextID = new Dictionary<Type, uint>();
            ServerTypeToIKS = new Dictionary<Type, IdempotencyKeyStore>();
            ClientTypeToIKS = new Dictionary<Type, IdempotencyKeyStore>();
            CreateFromClient = new PacketACKCreator<PacketACK>[256];
            CreateFromServer = new PacketACKCreator<PacketACK>[256];
        }

        private static Dictionary<Type, IdempotencyKeyStore> GetStore(bool serverSide) {
            if(serverSide) {
                return ServerTypeToIKS;
            }
            else {
                return ClientTypeToIKS;
            }
        }

        /// <summary>
        /// Gets the next ACK ID for a specific packet type
        /// </summary>
        /// <typeparam name="T">The packet type</typeparam>
        /// <returns></returns>
        public static uint GetNextID<T>() where T : PacketACK {
            return GetNextID(typeof(T));
        }

        /// <summary>
        /// Gets the next ACK ID for a specific packet type
        /// </summary>
        /// <param name="type">The packet type</param>
        /// <returns></returns>
        public static uint GetNextID(Type type) {
            uint nextKey;
            if (TypeToNextID.TryGetValue(type, out nextKey)) {
                if (nextKey == IDEMPOTENCY_MAX) {
                    // Ran out of keys... not sure what to do next :(
                    // Ignore comments below... 
                    throw new Exception("Cannot get a next key; all 536 million keys have been used");
                    // for the sake of why not... just wrap around to the minimum if there are
                    // no more available IDs. realistically... this should never happen,
                    // but it's possible if the runtime is very long and a lot of ACK packets
                    // are sent
                    // nextKey = IDEMPOTENCY_MIN;
                    // ClientTypeToIKS[type].Clear();
                    // ServerTypeToIKS[type].Clear();
                }
                else {
                    nextKey++;
                }
            }
            else {
                nextKey = IDEMPOTENCY_MIN;
            }

            return TypeToNextID[type] = nextKey;
        }

        /// <summary>
        /// Checks if the given idempotency key has already been handled
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsHandled<T>(uint id, bool serverSide) where T : PacketACK {
            return GetStore(serverSide)[typeof(T)].HasKey(id);
        }

        public static bool IsHandled(PacketACK packet, bool serverSide) {
            return GetStore(serverSide)[packet.GetType()].HasKey(packet.Key);
        }

        public static bool IsHandled(Type type, uint key, bool serverSide) {
            if (typeof(PacketACK).IsAssignableFrom(type)) {
                return GetStore(serverSide)[type].HasKey(key);
            }
            else {
                throw new Exception($"Type must be of {typeof(PacketACK).Name}");
            }
        }

        public static bool SetHandled<T>(uint id, bool serverSide) {
            return GetStore(serverSide)[typeof(T)].Put(id);
        }

        public static bool SetHandled(PacketACK packet, bool serverSide) {
            return GetStore(serverSide)[packet.GetType()].Put(packet.Key);
        }

        public static bool SetUnhandled<T>(uint id, bool serverSide) {
            return GetStore(serverSide)[typeof(T)].Remove(id);
        }

        public static bool SetUnhandled(PacketACK packet, bool serverSide) {
            return GetStore(serverSide)[packet.GetType()].Remove(packet.Key);
        }

        /// <summary>
        /// The code which this packet is destined for
        /// </summary>
        public DestinationCode Destination { get; set; }

        /// <summary>
        /// The idempotency key
        /// </summary>
        public uint Key { get; set; }

        protected PacketACK(DestinationCode destination, uint idempotencyKey) {
            if (idempotencyKey < IDEMPOTENCY_MIN || idempotencyKey > IDEMPOTENCY_MAX) {
                throw new PacketCreationFailure($"Key ({idempotencyKey}) was not between {IDEMPOTENCY_MIN} and {IDEMPOTENCY_MAX}");
            }

            this.Key = idempotencyKey;
            this.Destination = destination;
        }

        protected PacketACK() {
            this.Destination = DestinationCode.ToServer;
        }

        /// <summary>
        /// Registers an ACK packet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="createFromClient">
        /// The creator used to create packets that get processed internally (by an ACK packet listener)
        /// <para>
        /// Param 1 = the destination code (it is automatically set after this func is invoked)
        /// Param 2 = the idempotency token/key (it is automatically set after this func is invoked)
        /// Param 3 = the data input
        /// Param 4 = the packet's length (not including the packet or ACK header)
        /// </para>
        /// </param>
        /// <param name="createFromServer"></param>
        public static void RegisterACKPacket<T>(byte id, PacketACKCreator<T> createFromClient, PacketACKCreator<T> createFromServer) where T : PacketACK {
            ServerTypeToIKS[typeof(T)] = new IdempotencyKeyStore();
            ClientTypeToIKS[typeof(T)] = new IdempotencyKeyStore();
            CreateFromClient[id] = createFromClient;
            CreateFromServer[id] = createFromServer;
            RegisterPacket<T>(id, (input, length) => {
                return (T) CreatePacketACK(id, input, (ushort) (length - ACK_HEADER_SIZE));
            });
        }

        /// <summary>
        /// Creates a <see cref="PacketACK"/>
        /// </summary>
        /// <param name="id">The packet ID</param>
        /// <param name="input">The length of </param>
        /// <param name="length">The length of the ACK packet (not including the ACK header)</param>
        /// <returns></returns>
        private static PacketACK CreatePacketACK(byte id, IDataInput input, ushort length) {
            uint keyAndDestination = input.ReadInt();
            uint key = (keyAndDestination >> 3) & IDEMPOTENCY_MAX;
            DestinationCode code = (DestinationCode) (keyAndDestination & 0b00000111);
            if (code == DestinationCode.ToServer) {
                PacketACKCreator<PacketACK> creator = CreateFromClient[id];
                if (creator == null) {
                    throw new Exception($"Missing creator for ID {id}");
                }

                PacketACK ack = creator(DestinationCode.ServerACK, key, input, length);
                ack.Key = key;
                ack.Destination = DestinationCode.ServerACK;
                return ack;
            }
            else if (code == DestinationCode.ToClient) {
                PacketACKCreator<PacketACK> creator = CreateFromServer[id];
                if (creator == null) {
                    throw new Exception($"Missing creator for ID {id}");
                }

                PacketACK ack = creator(DestinationCode.ToClient, key, input, length);
                ack.Key = key;
                ack.Destination = DestinationCode.ToClient;
                return ack;
            }
            else if (code == DestinationCode.ServerACK) {
                throw new PacketCreationFailure($"Received ACK packet with destination ServerACK (ID = {id}, Len = {length}, Key = {key}, Full data = {keyAndDestination})");
            }
            else {
                throw new PacketCreationFailure($"Invalid ACK DestinationCode '{code}' (ID = {id}, Len = {length}, Key = {key}, Full data = {keyAndDestination})");
            }
        }

        /// <summary>
        /// This is used by an <see cref="ACKProcessor{TPacketACK}"/>, to determind if this packet has expired (meaning it has new data)
        /// <para>
        /// This is only used if the client has send a repeated packet to the server, and it's been 
        /// longer than <see cref="ACKProcessor{TPacketACK}.IgnoreRepeatTime"/>. If so, this 
        /// packet is invalidated (removed from the cache) and a new packet may be constructed
        /// </para>
        /// <para>
        /// This is <see langword="false"/> by default, because there are very little reasons to have a 
        /// cached packet expire
        /// </para>
        /// </summary>
        /// <returns></returns>
        public virtual bool HasExpired() {
            return false;
        }

        public sealed override ushort GetLength() {
            DestinationCode destination = this.Destination;
            if (destination == DestinationCode.ToServer) {
                return (ushort) (4 + GetLengthToServer());
            }
            else if (destination == DestinationCode.ToClient) {
                return (ushort) (4 + GetLengthToClient());
            }
            else {
                throw new Exception("Cannot get the length of an ACK packet that isn't to the server or client");
            }
        }

        public sealed override void Write(IDataOutput output) {
            DestinationCode code = this.Destination;
            if (code == DestinationCode.ServerACK) {
                throw new ACKException($"Attempted to write {DestinationCode.ServerACK} packet (Packet should've been recreated)");
            }
            else if (code == DestinationCode.ToServer || code == DestinationCode.ToClient) {
                // always assuming the key is smaller than IDEMPOTENCY_MAX
                output.WriteInt((this.Key << 3) | ((uint) code));
                if (code == DestinationCode.ToServer) {
                    WriteToServer(output);
                }
                else if (code == DestinationCode.ToClient) {
                    WriteToClient(output);
                }
                else {
                    throw new ACKException("Invalid destination code, it was not to the client or server");
                }
            }
            else {
                throw new ACKException("Invalid destination code, it was not to the client or server");
            }
        }

        /// <summary>
        /// Writes the custom data in this packet to the given <see cref="IDataOutput"/> to the client.
        /// The packet headers (ID, length, destination code + ACK ID) are written automatically
        /// </summary>
        public abstract void WriteToServer(IDataOutput output);

        /// <summary>
        /// Writes the custom data in this packet to the given <see cref="IDataOutput"/> to the server.
        /// The packet headers (ID, length, destination code + ACK ID) are written automatically
        /// </summary>
        public abstract void WriteToClient(IDataOutput output);

        /// <summary>
        /// The length/size of this ACK packet being sent to the server, in bytes. 
        /// This should only include custom packet data, not the header (id + len), nor the ACK header (destination code + ack id)
        /// </summary>
        /// <returns>A value between 0 and 65531</returns>
        public abstract ushort GetLengthToServer();

        /// <summary>
        /// The length/size of this ACK packet being sent to the client, in bytes. 
        /// This should only include custom packet data, not the header (id + len), nor the ACK header (destination code + ack id)
        /// </summary>
        /// <returns>A value between 0 and 65531</returns>
        public abstract ushort GetLengthToClient();
    }
}
