using System;

namespace REghZyIOWrapperV2.Packeting.Handling {
    public class GeneralHandler : IHandler {
        private readonly Predicate<Packet> handler;

        public GeneralHandler(Predicate<Packet> handler) {
            if (handler == null) {
                throw new NullReferenceException("Handler cannot be null");
            }

            this.handler = handler;
        }

        public bool CanHandle(Packet packet) {
            return true;
        }

        public bool Handle(Packet packet) {
            return this.handler(packet);
        }
    }
}
