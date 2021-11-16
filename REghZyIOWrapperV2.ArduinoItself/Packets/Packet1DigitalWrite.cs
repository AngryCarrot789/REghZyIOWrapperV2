using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Arduino.Packets {
    [PacketImplementation]
    public class Packet1DigitalWrite : Packet {
        public byte Pin { get; }
        public bool State { get; }

        public Packet1DigitalWrite(int pin, bool state) {
            this.Pin = (byte) pin;
            this.State = state;
        }

        static Packet1DigitalWrite() {
            RegisterPacket<Packet1DigitalWrite>(1, (input, length) => {
                byte data = input.ReadByte();
                return new Packet1DigitalWrite((data >> 1) & 127, (data & 1) == 1);
            });
        }

        public override ushort GetLength() {
            return 1;
        }

        public override void Write(IDataOutput output) {
            // pin max value = 127
            output.WriteByte((byte) ((this.Pin << 1) | (this.State == true ? 1 : 0)));
        }
    }
}
