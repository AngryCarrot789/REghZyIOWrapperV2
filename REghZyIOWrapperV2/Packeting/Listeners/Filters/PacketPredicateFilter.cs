using System;
using REghZyIOWrapperV2.Packeting.Listeners.Filters;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapper.Listeners.Filters {
    public class PacketPredicateFilter : PacketFilter {
        private readonly Predicate<Packet> predicate;

        public Predicate<Packet> Predicate {
            get => this.predicate;
        }

        public PacketPredicateFilter(Predicate<Packet> predicate) {
            if (predicate == null) {
                throw new NullReferenceException("Predicate cannot be null");
            }

            this.predicate = predicate;
        }

        public bool Accept(Packet packet) {
            return this.predicate(packet);
        }
    }
}
