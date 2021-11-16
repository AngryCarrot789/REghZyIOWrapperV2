using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting.ACK {
    [PacketImplementation]
    public class Packet3HardwareInfo : PacketACK {
        public enum HardwareInfos : byte {
            HardwareName = 1, // request the name of the hardware
            SerialPortName = 2  // The COM port the hardware is on
        }

        public HardwareInfos Code { get; }

        public string Information { get; }

        public Packet3HardwareInfo(HardwareInfos code, string information = null) {
            this.Code = code;
            this.Information = information;
        }

        static Packet3HardwareInfo() {
            RegisterACKPacket<Packet3HardwareInfo>(3, (c, key, input, len) => {
                return new Packet3HardwareInfo(input.ReadEnum8<HardwareInfos>());
            }, (c, key, input, len) => { 
                return new Packet3HardwareInfo(input.ReadEnum8<HardwareInfos>(), PacketUtils.ReadStringWL(input));
            });
        }

        public override ushort GetLengthACK() {
            if (this.Information == null) {
                return 1;
            }
            else {
                return (ushort) (1 + this.Information.GetBytesWL());
            }
        }

        public override void WriteToClient(IDataOutput output) {
            // faster to cast to a byte than to invoke multiple other methods to convert using pointers and stuff
            output.WriteByte((byte) this.Code);
        }

        public override void WriteToServer(IDataOutput output) {
            // faster to cast to a byte than to invoke multiple other methods to convert using pointers and stuff
            output.WriteByte((byte) this.Code);
            if (this.Information != null) {
                PacketUtils.WriteStringWL(this.Information, output);
            }
        }
    }
}
