using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Packeting.Exceptions;
using REghZyIOWrapperV2.Packeting.Handling;

namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// A helper class for managing ACK packets (aka packets that can be sent, and 
    /// then receive the same packet (but with extra "details") back)
    /// 
    /// <para>
    /// From the code point of view, <see cref="DestinationCode.ToClient"/> is not us, we are the server.
    /// </para>
    /// <para>
    /// <see cref="DestinationCode.ToServer"/> is us, and any packet that sends in that direction will usually contain data
    /// </para>
    /// <para>
    /// We send packets to the client. The client, from their point of view, is the 
    /// server, and we end up being the client. So it's about perspective
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of ACK packet</typeparam>
    public abstract class ACKProcessor<T> where T : PacketACK {
        /// <summary>
        /// Maps the idempoency key to the originally sent ACK packet (aka the ToClient one)
        /// <para>
        /// This is used so that the packet cannot be sent again, 
        /// if a responce is not received after any time interval
        /// </para>
        /// </summary>
        private readonly Dictionary<uint, T> LastSendPacket = new Dictionary<uint, T>();

        /// <summary>
        /// Maps the idempotency key to a packet. When ACK packets of type <see cref="T"/> are received,
        /// they are shoved in here, and then they can be fetched from the <see cref="ReceivePacketAsync(uint)"/> method 
        /// </summary>
        private readonly Dictionary<uint, T> LastReadPacket = new Dictionary<uint, T>();

        protected readonly PacketSystem packetSystem;

        private bool isRequestUnderWay;

        private bool allowExtraPacket;

        /// <summary>
        /// A value indicating whether extra packets are allowed to be sent if the ACK packets are not received within half a second or so
        /// </summary>
        public bool SendExtraPacket {
            get => this.allowExtraPacket;
            set => this.allowExtraPacket = value;
        }

        public PacketSystem PacketSystem {
            get => this.packetSystem;
        }

        protected ACKProcessor(PacketSystem packetSystem, Priority priority = Priority.HIGHEST) {
            if (packetSystem == null) {
                throw new ArgumentNullException(nameof(packetSystem), "Packet system cannot be null");
            }

            this.packetSystem = packetSystem;
            this.packetSystem.RegisterHandler<T>(OnPacketReceived, priority);
            this.isRequestUnderWay = false;
        }

        /// <summary>
        /// Sets up the given packet's key and destination for you, and sends the packet, returning the ID of the packet
        /// <para>
        /// This packet is now participating in the ACK transaction, therefore, no other packets should be sent until
        /// the responce has been received (aka a packet, that this processor processes, 
        /// is received in direction <see cref="DestinationCode.ToServer"/>)
        /// </para>
        /// </summary>
        /// <param name="packet"></param>
        public uint SendRequest(T packet) {
            if (this.isRequestUnderWay) {
                throw new Exception("ACK Packet is already in transit");
            }

            this.isRequestUnderWay = true;
            packet.Key = PacketACK.GetNextID<T>();
            packet.Destination = DestinationCode.ToClient;
            LastSendPacket[packet.Key] = packet;
            this.packetSystem.EnqueuePacket(packet);
            return packet.Key;
        }

        /// <summary>
        /// A helper function for sending a packet back to the server, 
        /// automatically filling in the key and destination for you
        /// <para>
        /// The packet "toServer" should contain all of the custom information 
        /// that the packet should contain.............................. yep
        /// </para>
        /// </summary>
        /// <param name="fromServer"></param>
        /// <param name="toServer"></param>
        protected void SendBackFromACK(T fromServer, T toServer) {
            toServer.Key = fromServer.Key;
            toServer.Destination = DestinationCode.ToServer;
            this.packetSystem.EnqueuePacket(toServer);
        }

        /// <summary>
        /// Called when the packet is received from the server or client
        /// <para>
        /// This is the client if the <see cref="PacketACK.Destination"/> is <see cref="DestinationCode.ClientACK"/>, and contains request information
        /// </para>
        /// <para>
        /// This is the server if the <see cref="PacketACK.Destination"/> is <see cref="DestinationCode.ToServer"/>, and contains client information
        /// </para>
        /// </summary>
        /// <param name="packet"></param>
        private bool OnPacketReceived(T packet) {
            // acknowledgement
            if (packet.Destination == DestinationCode.ClientACK) {
                // We are the client, so process the data
                return OnProcessPacketToClientACK(packet);
            }
            else if (packet.Destination == DestinationCode.ToServer) {
                this.isRequestUnderWay = false;
                // client sent the data back to us
                if (PacketACK.IsHandled(packet)) {
                    // dont handle the packet again
                    return true;
                }

                // packet contains actual good information!!! 
                this.LastReadPacket[packet.Key] = packet;
                if (OnProcessPacketToServer(packet)) {
                    PacketACK.SetHandled(packet);
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                // bug???
                throw new ACKException("Received hardware info packet destination was not ACK or ToServer");
            }
        }

        private static long SystemMillis() {
            return Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Waits until the last received packet is no longer null (meaning a new ACK packet has arrived), and then returns it as the task's result
        /// <para>
        /// This will wait forever until the packet has arrived
        /// </para>
        /// </summary>
        public async Task<T> ReceivePacketAsync(uint id) {
            int readAttempts = 0;
            long start = SystemMillis();
            while (true) {
                if (this.LastReadPacket.TryGetValue(id, out T packet) && packet != null) {
                    this.LastSendPacket.Remove(id);
                    return packet;
                }

                readAttempts++;
                if (readAttempts > 50) { // saves constantly calling SystemMills which could be slightly slow
                    if ((SystemMillis() - start) > 1000) {
                        if (LastSendPacket.TryGetValue(id, out T pkt)) {
                            Console.WriteLine("Did not receive after 1000ms... writing packet again");
                            this.packetSystem.EnqueuePacket(pkt);
                        }
                    }

                    readAttempts = 0;
                }

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Sends the given packet, and waits for a responce
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public async Task<T> MakeRequestAsync(T packet) {
            return await ReceivePacketAsync(SendRequest(packet));
        }

        /// <summary>
        /// This will be called if this is the client. It is called when we receive a packet from the server, 
        /// aka the mid-way between getting and receiving data (that is, if the ACK packet is used for that)
        /// <para>
        /// If the server wanted data (which is usually the usage for ACK packets), the packet in the parameters  will usually 
        /// contain request information, which will be used to fill in data for a new packet, and then be sent to the server
        /// </para>
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="IHandler.Handle(Packet)"/>),
        /// <see langword="false"/> if the packet shouldn't be handled, and can possibly be sent to other handlers/listeners
        /// </returns>
        protected abstract bool OnProcessPacketToClientACK(T packet);

        /// <summary>
        /// This will only be called if this is the server. It is called when we (the server) 
        /// receive a packet from the client (usually after the client runs <see cref="OnProcessPacketToClientACK"/>)
        /// <para>
        /// If the server wanted data (which is usually the usage for ACK packets), 
        /// the packet in the parameters will usually contain the information requested
        /// </para>
        /// <para>
        /// Usually, this method is empty (because you usually use the <see cref="ReceivePacketAsync(uint)"/> method, 
        /// which will usually execute before this method... usually (it's async so there's a chance it will return after the method below))
        /// </para>
        /// <para>
        /// Usually, this should return <see langword="true"/> because you typically use <see cref="ReceivePacketAsync(uint)"/> to process the packets,
        /// and you dont want outside handlers and listeners having a look
        /// </para>
        /// <para>
        /// And if this returns <see langword="false"/>, then it's
        /// idempotency key will NOT be stored, and therefore the exact same packet could be 
        /// processed multiple times, which is dangerous if it's a crucial transaction</para>
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="IHandler.Handle(Packet)"/>),
        /// and the idempotency key will be stored.
        /// 
        /// <see langword="false"/> if the packet shouldn't be handled, and can possibly be 
        /// sent to other handlers/listeners, and the idempotency key won't be stored
        /// </returns>
        protected abstract bool OnProcessPacketToServer(T packet);
    }
}
