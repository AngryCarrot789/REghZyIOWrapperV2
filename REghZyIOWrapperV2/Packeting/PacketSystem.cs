using System;
using System.Collections.Generic;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Packeting.Listeners;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting {
    /// <summary>
    /// A packet system is what handles sending and receiving packets, and delivering received packets to listeners
    /// </summary>
    public abstract class PacketSystem {
        public BaseConnection Connection { get; }

        private List<IPacketListener> listeners;

        public PacketSystem(BaseConnection connection) {
            this.Connection = connection;
            this.listeners = new List<IPacketListener>(32);
        }

        /// <summary>
        /// Registers a listener, allowing it to receive received packets by this packet system
        /// <para>
        /// These listeners will maintain their order in which they are registered
        /// </para>
        /// </summary>
        /// <param name="listener"></param>
        public void RegisterListener(IPacketListener listener) {
            if (this.listeners.Contains(listener)) {
                throw new Exception("This packet system already contained the given packet listener");
            }

            this.listeners.Add(listener);
        }

        /// <summary>
        /// Unregisters a listener, stopping it from receiving received packets by this packet system
        /// <para>
        /// These listeners will maintain their order in which they are registered
        /// </para>
        /// </summary>
        /// <param name="listener"></param>
        public void UnregisterListener(IPacketListener listener) {
            if (this.listeners.Remove(listener)) {
                return;
            }

            throw new Exception("Couldn't unregister packet listener, because it was never registered!");
        }

        /// <summary>
        /// Called when a packet is received
        /// </summary>
        /// <param name="packet"></param>
        public void OnPacketReceived(Packet packet) {
            foreach (IPacketListener listener in this.listeners) {
                if (listener.OnReceivePacket(packet)) {
                    return;
                }
            }
        }

        /// <summary>
        /// Sends a packet to the connection in this packet system
        /// </summary>
        /// <param name="packet">The packet to send (non-null)</param>
        public void SendPacket(Packet packet) {
            Packet.WritePacket(packet, this.Connection.Output);
            this.Connection.Flush();
        }
    }
}