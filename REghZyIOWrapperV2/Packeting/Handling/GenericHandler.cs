using System;
using REghZyIOWrapperV2.Packeting.Packets;

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

        public bool CanHandle(Packet packet) {
            return packet.GetType().Equals(this.type);
        }

        public bool Handle(Packet packet) {
            return this.handler(packet);
        }
    }

    public class GenericHandler<T> : GenericHandler where T : Packet {
        public GenericHandler(Predicate<T> handler) : base(typeof(T), (Predicate<Packet>) handler) {
            
        }

        public GenericHandler(Predicate<Packet> handler) : base(typeof(T), handler) {

        }
    }
}
