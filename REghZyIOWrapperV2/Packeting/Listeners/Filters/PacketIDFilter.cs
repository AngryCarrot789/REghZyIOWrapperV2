using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Listeners.Filters {
    /// <summary>
    /// Accepts packets with a specific ID
    /// </summary>
    public class PacketIDFilter : PacketFilter {
        private readonly int acceptedId;

        public int AcceptedID {
            get => this.acceptedId;
        }

        public PacketIDFilter(int acceptedId) {
            this.acceptedId = acceptedId;
        }

        public bool Accept(Packet packet) {
            return Packet.GetPacketID(packet) == this.acceptedId;
        }
    }
}
