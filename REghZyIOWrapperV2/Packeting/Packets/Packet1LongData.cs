using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting.Packets {
    /// <summary>
    /// A simple packet implementation that only writes an unsigned
    /// long (aka 8 bytes) that could be used for bit things
    ///
    /// <para>
    /// This probably doesn't have any actual use apart from showing how to use the packet class. What would process this data.....?
    /// </para>
    /// </summary>
    [PacketImplementation]
    public class Packet1LongData : Packet {
        public ulong Data { get; set; }

        static Packet1LongData() {
            RegisterPacket(1, (input) => new Packet1LongData(input.ReadLong()));
        }

        public Packet1LongData(ulong data = 0) {
            this.Data = data;
        }

        public override void Write(IDataOutput writer) {
            writer.WriteLong(this.Data);
        }
    }
}