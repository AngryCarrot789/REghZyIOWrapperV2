using System;
using System.Collections.Generic;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Packeting.Listeners;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting {
    /// <summary>
    /// A packet system is what handles sending and receiving packets, and delivering received packets to listeners
    /// </summary>
    public class PacketSystem {
        /// <summary>
        /// The connection that this packet system uses
        /// </summary>
        public BaseConnection Connection { get; }

        /// <summary>
        /// The packet handler
        /// </summary>
        public PacketHandler Handler { get; }

        public PacketSystem(BaseConnection connection, PacketHandler handler) {
            this.Connection = connection;
            this.Handler = handler;
        }

        /// <summary>
        /// Reads the next available packet
        /// </summary>
        /// <returns>
        /// True if a packet was read, otherwise false (if there wasn't enough data available)
        /// </returns>
        public bool ReadNextPacket() {
            int buffer = this.Connection.Stream.BytesAvailable;
            if (buffer < 1) {
                return false;
            }

            Packet packet;

            try {
                packet = Packet.ReadPacketHeadAndTail(this.Connection.Stream);
            }
            catch (Exception e) {
                throw new Exception($"Failed to read packet. Buffer before: {buffer}, After: {this.Connection.Stream.BytesAvailable}", e);
            }

            HandlePacketReceived(packet);
            return true;
        }

        /// <summary>
        /// Handles a packet that has been received
        /// </summary>
        /// <param name="packet"></param>
        public void HandlePacketReceived(Packet packet) {
            PacketHandler handler = this.Handler;
            if (handler != null) {
                try {
                    handler.OnReceivePacket(packet);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to handle packet", e);
                }
            }
        }

        /// <summary>
        /// Sends a packet to the connection in this packet system
        /// </summary>
        /// <param name="packet">The packet to send (non-null)</param>
        public void SendPacket(Packet packet) {
            Packet.WritePacket(packet, this.Connection.Stream.Output);
        }
    }
}