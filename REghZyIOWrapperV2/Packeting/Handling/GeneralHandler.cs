using System;

namespace REghZyIOWrapperV2.Packeting.Handling {
    public class GeneralHandler : IHandler {
        private readonly Predicate<Packet> handler;
        private readonly Predicate<Packet> canProcess;

        public GeneralHandler(Predicate<Packet> handler, Predicate<Packet> canProcess = null) {
            if (handler == null) {
                throw new NullReferenceException("Handler cannot be null");
            }

            this.handler = handler;
            if (canProcess == null) {
                this.canProcess = RetTrue;
            }
            else {
                this.canProcess = canProcess;
            }
        }

        public bool CanProcess(Packet packet) {
            return this.canProcess(packet);
        }

        public bool Handle(Packet packet) {
            return this.handler(packet);
        }

        private static bool RetTrue(Packet packet) {
            return true;
        }
    }
}
