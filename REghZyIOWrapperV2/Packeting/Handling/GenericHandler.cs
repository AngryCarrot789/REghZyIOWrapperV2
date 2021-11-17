using System;

namespace REghZyIOWrapperV2.Packeting.Handling {
    public class GenericHandler : IHandler {
        private readonly Type type;
        private readonly Predicate<Packet> handler;

        public GenericHandler(Type type, Predicate<Packet> handler) {
            if (handler == null) {
                throw new NullReferenceException("Handler cannot be null");
            }

            this.type = type;
            this.handler = handler;
        }

        public bool CanProcess(Packet packet) {
            return packet.GetType().Equals(this.type);
        }

        public bool Handle(Packet packet) {
            return this.handler(packet);
        }
    }

    public class GenericHandler<T> : IHandler where T : Packet {
        private readonly Predicate<T> handler;

        public GenericHandler(Predicate<T> handler) {
            if (handler == null) {
                throw new NullReferenceException("Handler cannot be null");
            }

            this.handler = handler;
        }

        public bool CanProcess(Packet packet) {
            return packet.GetType().Equals(typeof(T));
        }

        public bool Handle(Packet packet) {
            return this.handler((T) packet);
        }
    }
}
