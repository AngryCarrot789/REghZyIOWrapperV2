using System;
using REghZyIOWrapperV2.Packeting.Listeners.Filters;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting.Listeners {
    /// <summary>
    /// A class which holds a reference to a callback method that should be called
    /// if a packet received is accepted by the contained <see cref="PacketFilter"/>
    /// <para>
    /// By default, the <see cref="OnReceivePacket(Packet)"/> function returns <see langword="false"/>, so
    /// any other listener that listens to the same packets will receive those same packets
    /// </para>
    /// </summary>
    // Naming convention i like is 'PacketListener...', where ... is the type of packet listener
    public class FilteredPacketListener : IPacketListener {
        private bool cancelOnReceive;

        /// <summary>
        /// The filter that will be used to see if this <see cref="FilteredPacketListener"/> is allowed to be notified of
        /// a <see cref="Packet"/> received by the <see cref="SerialConnection"/>
        /// </summary>
        public PacketFilter Filter { get; }

        /// <summary>
        /// The callback function that is called if the <see cref="PacketFilter"/> allows the <see cref="Packet"/>
        /// </summary>
        public Action<Packet> OnPackedReceived { get; }

        /// <summary>
        /// If <see langword="true"/>, when a packet is received by this listener (and obviously accepted by the filter), the same packet
        /// wont be sent to any more packet listeners.
        /// <para>
        /// If <see langword="false"/>, any other packet listeners will continue to listen to the same packet even after this one has received it
        /// </para>
        /// <para>
        /// This is <see langword="false"/> by default
        /// </para>
        /// </summary>
        public bool CancelOnReceive {
            get => this.cancelOnReceive;
        }

        public FilteredPacketListener(PacketFilter filter, Action<Packet> onPacketReceived, bool cancelOnReceive = false) {
            if (filter == null) {
                throw new NullReferenceException("Filter cannot be null");
            }

            if (onPacketReceived == null) {
                throw new NullReferenceException("Packet received callback cannot be null");
            }

            this.Filter = filter;
            this.OnPackedReceived = onPacketReceived;
            this.cancelOnReceive = cancelOnReceive;
        }

        public bool OnReceivePacket(Packet packet) {
            if (this.Filter.Accept(packet)) {
                this.OnPackedReceived(packet);
                return this.cancelOnReceive;
            }

            return false;
        }
    }
}