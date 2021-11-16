using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Handling {
    /// <summary>
    /// A map, mapping a priority to a collection of listeners and handlers
    /// <para>
    /// Listeners of the same priority as handlers will receive packets first, e.g a listener with priotity 1 
    /// will receive packets first, then handlers or priority 1 will receive packets. But, listeners of priority 2 will
    /// receive packets AFTER handlers of priority 1
    /// </para>
    /// <para>
    /// So the order of received packets being delivered is: 
    /// Listeners(HIGHEST), Handers(HIGHEST), Listeners(HIGH), Handers(HIGH), Listeners(NORMAL),
    /// Handers(NORMAL), Listeners(LOW), Handers(LOW), Listeners(LOWEST), Handers(LOWEST), 
    /// </para>
    /// <para>
    /// This means listeners have an overall higher priority than handlers of the same 
    /// priority level, which may be useful for "sniffing" packets before they get handled
    /// </para>
    /// </summary>
    public class PriorityMap {
        private readonly List<IHandler>[] handlers;
        private readonly List<IListener>[] listeners;

        public PriorityMap() {
            this.handlers = new List<IHandler>[6];
            this.listeners = new List<IListener>[6];

            this.handlers[(int) Priority.HIGHEST] = new List<IHandler>();
            this.handlers[(int) Priority.HIGH] = new List<IHandler>();
            this.handlers[(int) Priority.NORMAL] = new List<IHandler>();
            this.handlers[(int) Priority.LOW] = new List<IHandler>();
            this.handlers[(int) Priority.LOWEST] = new List<IHandler>();

            this.listeners[(int) Priority.HIGHEST] = new List<IListener>();
            this.listeners[(int) Priority.HIGH] = new List<IListener>();
            this.listeners[(int) Priority.NORMAL] = new List<IListener>();
            this.listeners[(int) Priority.LOW] = new List<IListener>();
            this.listeners[(int) Priority.LOWEST] = new List<IListener>();
        }

        public List<IHandler> GetHandlers(Priority priority) {
            int index = (int) priority;
            if (index < 1 || index > 5) {
                throw new Exception("Missing priority (not 1-5): " + priority.ToString());
            }

            return this.handlers[index];
        }

        public List<IListener> GetListeners(Priority priority) {
            int index = (int) priority;
            if (index < 1 || index > 5) {
                throw new Exception("Missing priority (not 1-5): " + priority.ToString());
            }

            return this.listeners[index];
        }

        /// <summary>
        /// Delivers the given packet to all of the listeners and handlers, respecting their priority
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool DeliverPacket(Packet packet) {
            List<IHandler>[] handlers = this.handlers;
            List<IListener>[] listeners = this.listeners;
            foreach (IListener listener in listeners[1]) {
                ListenPacket(listener, packet);
            }

            foreach (IHandler handler in handlers[1]) {
                if (HandlePacket(handler, packet)) {
                    return true;
                }
            }

            foreach (IListener listener in listeners[2]) {
                ListenPacket(listener, packet);
            }

            foreach (IHandler handler in handlers[2]) {
                if (HandlePacket(handler, packet)) {
                    return true;
                }
            }

            foreach (IListener listener in listeners[3]) {
                ListenPacket(listener, packet);
            }

            foreach (IHandler handler in handlers[3]) {
                if (HandlePacket(handler, packet)) {
                    return true;
                }
            }

            foreach (IListener listener in listeners[4]) {
                ListenPacket(listener, packet);
            }

            foreach (IHandler handler in handlers[4]) {
                if (HandlePacket(handler, packet)) {
                    return true;
                }
            }

            foreach (IListener listener in listeners[5]) {
                ListenPacket(listener, packet);
            }

            foreach (IHandler handler in handlers[5]) {
                if (HandlePacket(handler, packet)) {
                    return true;
                }
            }

            return false;
        }

        private bool HandlePacket(IHandler handler, Packet packet) {
            if (handler.CanHandle(packet)) {
                if (handler.Handle(packet)) {
                    return true;
                }
            }

            return false;
        }

        private void ListenPacket(IListener listener, Packet packet) {
            listener.OnReceived(packet);
        }
    }
}
