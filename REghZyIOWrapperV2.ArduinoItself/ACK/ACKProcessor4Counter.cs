using REghZyIOWrapperV2.ArduinoItself.Packets;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;

namespace REghZyIOWrapperV2.ArduinoItself.ACK {
    public class ACKProcessor4Counter : ACKProcessor<Packet4Counter> {
        public ACKProcessor4Counter(PacketSystem packetSystem, Priority priority = Priority.HIGHEST) : base(packetSystem, priority) {

        }

        protected override bool OnProcessPacketToClient(Packet4Counter packet) {
            return true;
        }

        protected override bool OnProcessPacketToServerACK(Packet4Counter packet) {
            SendBackFromACK(packet);
            return true;
        }
    }
}
