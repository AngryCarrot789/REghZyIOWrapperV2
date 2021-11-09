using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting.Packets {
    [PacketImplementation]
    public class Packet2YourInfo : Packet {
        public string Name;
        public int age;
        public string DateOfBirth;

        static Packet2YourInfo() {
            RegisterPacket(2, (input) => {
                Packet2YourInfo info = new Packet2YourInfo();
                info.Name = input.ReadString(input.ReadShort());
                info.DateOfBirth = input.ReadString(input.ReadShort());
                info.age = (int) input.ReadInt();
                return info;
            });
        }

        public override void Write(IDataOutput writer) {
            writer.WriteShort((ushort) this.Name.Length);
            writer.WriteString(this.Name);
            writer.WriteShort((ushort) this.DateOfBirth.Length);
            writer.WriteString(this.Name);
            writer.WriteInt((uint) this.age);
        }
    }
}
