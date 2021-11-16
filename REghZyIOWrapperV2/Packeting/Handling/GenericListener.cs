using System;

namespace REghZyIOWrapperV2.Packeting.Handling {
    public class GenericListener<T> : IListener where T : Packet {
        private readonly Action<T> handler;

        public GenericListener(Action<T> handler) {
            this.handler = handler;
        }

        public GenericListener(Action<Packet> handler) {
            this.handler = handler;
        }

        public void OnReceived(Packet packet) {
            if (packet.GetType().Equals(typeof(T))) {
                this.handler((T) packet);
            }
        }
    }
}
