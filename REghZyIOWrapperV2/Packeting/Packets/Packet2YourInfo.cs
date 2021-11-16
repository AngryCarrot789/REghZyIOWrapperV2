using REghZyIOWrapperV2.Streams;
using REghZyIOWrapperV2.Utils;

namespace REghZyIOWrapperV2.Packeting.Packets {
    [PacketImplementation]
    public class Packet2YourInfo : Packet {
        public string Name;
        public int age;
        public string DateOfBirth;

        static Packet2YourInfo() {
            RegisterPacket(2, (input, len) => {
                Packet2YourInfo info = new Packet2YourInfo();
                info.Name = PacketUtils.ReadStringWL(input);
                info.age = (int) input.ReadInt();
                info.DateOfBirth = PacketUtils.ReadStringWL(input);
                return info;
            });
        }

        public override ushort GetLength() {
            return (ushort) (4 + this.Name.GetBytesWL() + this.DateOfBirth.GetBytesWL());
        }

        public override void Write(IDataOutput output) {
            PacketUtils.WriteStringWL(this.Name, output);
            output.WriteInt((uint) this.age);
            PacketUtils.WriteStringWL(this.DateOfBirth, output);
        }
    }
}
