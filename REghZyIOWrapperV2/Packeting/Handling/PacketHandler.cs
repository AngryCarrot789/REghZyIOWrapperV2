using System;
using System.Collections.Generic;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Handling {
    /// <summary>
    /// A class for registering packet listeners and handlers, and also for managing packets and delivering them correctly
    /// </summary>
    public class PacketHandler {
        private readonly PriorityMap map;

        public PacketHandler() {
            this.map = new PriorityMap();
        }

        /// <summary>
        /// Returns true if any packet was handled
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>
        /// Whether any packet was handled
        /// </returns>
        public bool OnReceivePacket(Packet packet) {
            return this.map.DeliverPacket(packet);
        }

        public void RegisterHandler(GeneralHandler handler, Priority priority = Priority.NORMAL) {
            this.map.GetHandlers(priority).Add(handler);
        }

        public void RegisterHandler(Predicate<Packet> handler, Priority priority = Priority.NORMAL) {
            this.map.GetHandlers(priority).Add(new GeneralHandler(handler));
        }

        public void RegisterHandler<T>(Predicate<T> handler, Priority priority = Priority.NORMAL) where T : Packet {
            this.map.GetHandlers(priority).Add(new GenericHandler<T>(handler));
        }

        public void RegisterListener(GeneralListener handler, Priority priority = Priority.NORMAL) {
            this.map.GetListeners(priority).Add(handler);
        }

        public void RegisterListener(Action<Packet> handler, Priority priority = Priority.NORMAL) {
            this.map.GetListeners(priority).Add(new GeneralListener(handler));
        }

        public void RegisterListener<T>(Action<T> handler, Priority priority = Priority.NORMAL) where T : Packet {
            this.map.GetListeners(priority).Add(new GenericListener<T>(handler));
        }
    }
}
