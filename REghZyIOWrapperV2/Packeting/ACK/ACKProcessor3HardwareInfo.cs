using System;

namespace REghZyIOWrapperV2.Packeting.ACK {
    public class ACKProcessor3HardwareInfo : ACKProcessor<Packet3HardwareInfo> {
        private readonly Func<Packet3HardwareInfo.HardwareInfos, string> GetInfoCallback;

        public ACKProcessor3HardwareInfo(PacketSystem packetSystem, Func<Packet3HardwareInfo.HardwareInfos, string> getInfoCallback) : base(packetSystem) {
            this.GetInfoCallback = getInfoCallback;
        }

        // Sends a request for the given info, and returns the request ID
        public uint SendRequest(Packet3HardwareInfo.HardwareInfos info) {
            Packet3HardwareInfo packet = new Packet3HardwareInfo(info) {
                Destination = DestinationCode.ToClient, 
                Key = PacketACK.GetNextID<Packet3HardwareInfo>(),
            };

            this.SendPacket(packet);
            return packet.Key;
        }

        public override bool OnProcessPacketToClientACK(Packet3HardwareInfo packet) {
            string info = this.GetInfoCallback(packet.Code);
            this.SendPacket(new Packet3HardwareInfo(packet.Code, info) {
                Key = packet.Key,
                Destination = DestinationCode.ToServer
            });

            return true;
        }

        public override bool OnProcessPacketToServer(Packet3HardwareInfo packet) {
            return true;
        }
    }
}
