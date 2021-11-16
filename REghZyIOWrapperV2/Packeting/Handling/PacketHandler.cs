using System;
using System.Collections.Generic;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Handling {
    public class PacketHandler {
        private readonly List<IHandler> generalHandlers;
        private readonly List<IListener> generalListeners;

        public PacketHandler() {
            this.generalHandlers = new List<IHandler>(32);
            this.generalListeners = new List<IListener>(32);
        }

        /// <summary>
        /// Returns true if any packet was handled
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool OnReceivePacket(Packet packet) {
            foreach (IListener handler in this.generalListeners) {
                handler.OnReceived(packet);
            }

            foreach (IHandler handler in this.generalHandlers) {
                if (handler.CanHandle(packet)) {
                    if (handler.Handle(packet)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public void RegisterHandler(GeneralHandler handler) {
            this.generalHandlers.Add(handler);
        }

        public void RegisterHandler(Predicate<Packet> handler) {
            this.generalHandlers.Add(new GeneralHandler(handler));
        }

        public void RegisterHandler<T>(Predicate<T> handler) where T : Packet {
            this.generalHandlers.Add(new GenericHandler<T>(handler));
        }

        public void RegisterListener(GeneralListener handler) {
            this.generalListeners.Add(handler);
        }

        public void RegisterListener(Action<Packet> handler) {
            this.generalListeners.Add(new GeneralListener(handler));
        }

        public void RegisterListener<T>(Action<T> handler) where T : Packet {
            this.generalListeners.Add(new GenericListener<T>(handler));
        }
    }
}
