using System;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Handling {
    public class PacketTypeListener : IListener {
        private readonly Type type;
        private readonly Action<Packet> handler;

        public PacketTypeListener(Type type, Action<Packet> handler) {
            if (handler == null) {
                throw new NullReferenceException("Handler cannot be null");
            }

            this.type = type;
            this.handler = handler;
        }

        public void OnReceived(Packet packet) {
            if (packet.GetType().Equals(this.type)) {
                this.handler(packet);
            }
        }
    }
}
