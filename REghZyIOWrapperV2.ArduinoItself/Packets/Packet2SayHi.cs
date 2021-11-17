using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.ArduinoItself.Packets {
    [PacketImplementation]
    public class Packet2SayHi : Packet {
        public string Message { get; }

        public Packet2SayHi(string message = null) {
            this.Message = message;
        }

        static Packet2SayHi() {
            RegisterPacket(2, (input, len) => { return new Packet2SayHi(PacketUtils.ReadStringWL(input)); });
        }

        public override ushort GetLength() {
            return (ushort) (this.Message == null ? 0 : Message.GetBytesWL());
        }

        public override void Write(IDataOutput output) {
            if (this.Message != null) {
                PacketUtils.WriteStringWL(this.Message, output);
            }
        }
    }
}
