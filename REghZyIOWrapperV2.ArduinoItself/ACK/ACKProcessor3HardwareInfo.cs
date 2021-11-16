using System;
using REghZyIOWrapperV2.Arduino.Packets;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting;

namespace REghZyIOWrapperV2.Arduino.ACK {
    public class ACKProcessor3HardwareInfo : ACKProcessor<Packet3HardwareInfo> {
        private readonly Func<Packet3HardwareInfo.HardwareInfos, string> GetInfoCallback;

        public ACKProcessor3HardwareInfo(PacketSystem packetSystem, Func<Packet3HardwareInfo.HardwareInfos, string> getInfoCallback) : base(packetSystem) {
            this.GetInfoCallback = getInfoCallback;
        }

        // Sends a request for the given info, and returns the request ID
        public uint SendRequest(Packet3HardwareInfo.HardwareInfos info) {
            return SendRequest(new Packet3HardwareInfo(info));
        }

        protected override bool OnProcessPacketToClientACK(Packet3HardwareInfo packet) {
            string info = this.GetInfoCallback(packet.Code);
            SendBackFromACK(packet, new Packet3HardwareInfo(packet.Code, info));
            return true;
        }

        protected override bool OnProcessPacketToServer(Packet3HardwareInfo packet) {
            return true;
        }
    }
}