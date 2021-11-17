using System;
using System.Collections.Generic;
using System.Diagnostics;
using REghZyIOWrapperV2.Packeting.Exceptions;
using REghZyIOWrapperV2.Streams;

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

        /// <summary>
        /// A dictionary that maps the packet type to it's next available ID. If there isn't an entry for a
        /// packet, it is added (starting at <see cref="IDEMPOTENCY_MIN"/>)
        /// </summary>
        private static readonly Dictionary<Type, uint> TypeToNextID = new Dictionary<Type, uint>();

        private static readonly Dictionary<Type, IdempotencyKeyStore> TypeToProcessedIDs = new Dictionary<Type, IdempotencyKeyStore>();

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
                    // for the sake of why not... just wrap around to the minimum if there are
                    // no more available IDs. realistically... this should never happen,
                    // but it's possible if the runtime is very long and a lot of ACK packets
                    // are sent
                    nextKey = IDEMPOTENCY_MIN;
                    TypeToProcessedIDs[type].Clear();
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
        public static bool IsHandled<T>(uint id) {
            return TypeToProcessedIDs[typeof(T)].HasKey(id);
        }

        public static bool IsHandled(PacketACK packet) {
            return TypeToProcessedIDs[packet.GetType()].HasKey(packet.Key);
        }

        public static bool SetHandled<T>(uint id) {
            return TypeToProcessedIDs[typeof(T)].Put(id);
        }

        public static bool SetHandled(PacketACK packet) {
            return TypeToProcessedIDs[packet.GetType()].Put(packet.Key);
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

        }

        /// <summary>
        /// Registers an ACK packet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="fromServerToClientAck">
        /// The creator used to create packets that get processed internally (by an ACK packet listener)
        /// <para>
        /// Param 1 = the destination code (it is automatically set after this func is invoked)
        /// Param 2 = the idempotency token/key (it is automatically set after this func is invoked)
        /// Param 3 = the data input
        /// Param 4 = the packet's length (not including the packet or ACK header)
        /// </para>
        /// </param>
        /// <param name="fromClientToServer"></param>
        public static void RegisterACKPacket<T>(byte id, 
            Func<DestinationCode, uint, IDataInput, ushort, T> fromServerToClientAck, 
            Func<DestinationCode, uint, IDataInput, ushort, T> fromClientToServer) where T : PacketACK {
            TypeToProcessedIDs[typeof(T)] = new IdempotencyKeyStore();
            RegisterPacket<T>(id, (input, length) => {
                uint keyAndDestination = input.ReadInt();
                uint key = (keyAndDestination >> 3) & IDEMPOTENCY_MAX;
                DestinationCode code = (DestinationCode) (keyAndDestination & 0b00000111);
                if (code == DestinationCode.ToClient) {
                    T ack = fromServerToClientAck(DestinationCode.ClientACK, key, input, (ushort) (length - 4));
                    ack.Key = key;
                    ack.Destination = DestinationCode.ClientACK;
                    return ack;
                }
                else if (code == DestinationCode.ClientACK || code == DestinationCode.ToServer) {
                    T ack = fromClientToServer(DestinationCode.ToServer, key, input, (ushort) (length - 4));
                    ack.Key = key;
                    ack.Destination = DestinationCode.ToServer;
                    return ack;
                }
                else {
                    throw new PacketCreationFailure($"Invalid ACK DestinationCode '{code}' (ID = {id}, Len = {length}, Key = {key}, Full data = {keyAndDestination})");
                }
            });
        }

        public sealed override ushort GetLength() {
            return (ushort) (4 + GetLengthACK());
        }

        public sealed override void Write(IDataOutput output) {
            DestinationCode code = this.Destination;
            if (code == DestinationCode.ClientACK) {
                throw new ACKException("Attempted to write ClientACK packet (Packet should've been recreated using the DestinationCode.ToServer code)");
            }

            if (code == DestinationCode.ToClient || code == DestinationCode.ToServer) {
                // always assuming the key is smaller than IDEMPOTENCY_MAX
                output.WriteInt((this.Key << 3) | ((uint) code));
                if (code == DestinationCode.ToClient) {
                    WriteToClient(output);
                }
                else if (code == DestinationCode.ToServer) {
                    WriteToServer(output);
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
        public abstract void WriteToClient(IDataOutput output);

        /// <summary>
        /// Writes the custom data in this packet to the given <see cref="IDataOutput"/> to the server.
        /// The packet headers (ID, length, destination code + ACK ID) are written automatically
        /// </summary>
        public abstract void WriteToServer(IDataOutput output);

        /// <summary>
        /// The length or size of this ACK packet, in bytes. This should only include custom packet 
        /// data, not the header (id + len), nor the ACK header (destination code + ack id)
        /// </summary>
        /// <returns>A value between 0 and 65531</returns>
        public abstract ushort GetLengthACK();
    }
}
