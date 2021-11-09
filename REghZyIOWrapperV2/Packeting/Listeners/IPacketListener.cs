using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Listeners {
    /// <summary>
    /// A class that can listen to packets that have been received
    /// </summary>
    public interface IPacketListener {
        /// <summary>
        /// Called when a packet is received (this method may have been called for other listeners, depending on the registration order)
        /// </summary>
        /// <param name="packet">The packet to try and send to this listener</param>
        /// <returns>
        /// <see langword="true"/> if the packet is fully handled and shouldn't be processed or sent to any other listener.
        /// <see langword="false"/> if the same packet should be sent to other listeners
        /// </returns>
        bool OnReceivePacket(Packet packet);
    }
}