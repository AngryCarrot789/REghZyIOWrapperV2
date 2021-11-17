namespace REghZyIOWrapperV2.Packeting {
    public interface IPacketSystem {
        /// <summary>
        /// Adds the packet to the send queue
        /// </summary>
        /// <param name="packet"></param>
        void EnqueuePacket(Packet packet);

        /// <summary>
        /// Handles any packets in the read queue
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        int HandleReadPackets(int count = 10);
    }
}
