using System;
using System.Collections.Generic;
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
    /// </summary>
    /// <typeparam name="T">The type of ACK packet</typeparam>
    public abstract class ACKProcessor<T> where T : PacketACK {
        private readonly Dictionary<uint, T> LastReceivedPacket = new Dictionary<uint, T>();
        private readonly PacketSystem packetSystem;

        public PacketSystem PacketSystem {
            get => this.packetSystem;
        }

        protected ACKProcessor(PacketSystem packetSystem, Priority priority = Priority.HIGHEST) {
            if (packetSystem == null) {
                throw new ArgumentNullException(nameof(packetSystem), "Packet system cannot be null");
            }

            this.packetSystem = packetSystem;
            this.packetSystem.Handler.RegisterHandler<T>(OnPacketReceived, priority);
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
                // client sent the data back to us
                if (PacketACK.IsHandled(packet)) {
                    // dont handle the packet again
                    return true;
                }

                // packet contains actual good information!!! 
                this.LastReceivedPacket[packet.Key] = packet;
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

        /// <summary>
        /// Waits until the last received packet is no longer null (meaning a new ACK packet has arrived), and then returns it as the task's result
        /// <para>
        /// This will wait forever until the packet has arrived
        /// </para>
        /// </summary>
        public async Task<T> ReceivePacketAsync(uint id) {
            while (true) {
                if (this.LastReceivedPacket.TryGetValue(id, out T packet) && packet != null) {
                    return packet;
                }

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Sends the given packet to the connection (simply runs <see cref="Packeting.PacketSystem.SendPacket(Packets.Packet)"/> )
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(T packet) {
            this.packetSystem.SendPacket(packet);
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
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="IHandler.Handle(Packets.Packet)"/>),
        /// <see langword="false"/> if the packet shouldn't be handled, and can possibly be sent to other handlers/listeners
        /// </returns>
        public abstract bool OnProcessPacketToClientACK(T packet);

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
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="IHandler.Handle(Packets.Packet)"/>),
        /// and the idempotency key will be stored.
        /// 
        /// <see langword="false"/> if the packet shouldn't be handled, and can possibly be 
        /// sent to other handlers/listeners, and the idempotency key won't be stored
        /// </returns>
        public abstract bool OnProcessPacketToServer(T packet);
    }
}
