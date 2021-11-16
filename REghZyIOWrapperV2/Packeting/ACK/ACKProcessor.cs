using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Packeting.Listeners;

namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// A helper class for managing ACK packets (aka packets that can be sent, and 
    /// then receive the same packet (but with extra "details") back)
    /// </summary>
    /// <typeparam name="T">The type of ACK packet</typeparam>
    public abstract class ACKProcessor<T> where T : PacketACK {
        private readonly Dictionary<uint, T> LastReceivedPacket = new Dictionary<uint, T>();
        private readonly PacketSystem packetSystem;

        public PacketSystem PacketSystem {
            get => this.packetSystem;
        }

        protected ACKProcessor(PacketSystem packetSystem) {
            if (packetSystem == null) {
                throw new ArgumentNullException(nameof(packetSystem), "Packet system cannot be null");
            }

            this.packetSystem = packetSystem;
            this.packetSystem.Handler.RegisterHandler<T>(OnPacketReceived);
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
            if (packet.Destination == DestinationCode.ClientACK) {
                // acknowledgement
                return OnProcessPacketToClientACK(packet);
            }
            else if (packet.Destination == DestinationCode.ToServer) {
                // contains actual info
                this.LastReceivedPacket[packet.Key] = packet;
                return OnProcessPacketToServer(packet);
            }
            else {
                // bug???
                throw new Exception("Received hardware info packet destination was not ACK or ToServer");
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
        /// Sends the given packet to the connection (simply runs <see cref="PacketSystem.SendPacket(Packet)"/>
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
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="REghZyIOWrapperV2.Packeting.Handling.IHandler.Handle(Packets.Packet)"/>),
        /// <see langword="false"/> if the packet shouldn't be handled, and could possibly be sent to other handlers/listeners
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
        /// Usually, this method is empty (because you usually use the <see cref="ReceivePacketAsync(int)"/> 
        /// method, which will usually execute before this method... usually (it's async so there's a chance it will return after the method below))
        /// </para>
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="REghZyIOWrapperV2.Packeting.Handling.IHandler.Handle(Packets.Packet)"/>),
        /// <see langword="false"/> if the packet shouldn't be handled, and could possibly be sent to other handlers/listeners
        /// </returns>
        public abstract bool OnProcessPacketToServer(T packet);
    }
}
