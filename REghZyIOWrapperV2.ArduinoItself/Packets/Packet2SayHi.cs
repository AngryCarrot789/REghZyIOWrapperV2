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
            RegisterPacket(2, (input, len) => { 
                return new Packet2SayHi(len > 0 ? PacketUtils.ReadStringWL(input) : null); 
            });
        }

        public override ushort GetLength() {
            if (this.Message == null) {
                return 0;
            }
            else {
                return (ushort) PacketUtils.GetBytesWL(this.Message);
            }
        }

        public override void Write(IDataOutput output) {
            if (this.Message != null) {
                PacketUtils.WriteStringWL(this.Message, output);
            }
        }
    }
}
