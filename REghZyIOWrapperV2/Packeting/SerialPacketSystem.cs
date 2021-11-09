using System;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Packeting {
    public class SerialPacketSystem : PacketSystem {
        private readonly SerialConnection serial;

        public SerialPacketSystem(string port, int baud = 9600) : base(new SerialConnection(port, baud, 10)) {
            this.serial = (SerialConnection) this.Connection;
            this.serial.OnDataAvailable = this.OnDataAvailable;
        }

        private void OnDataAvailable() {
            int buffer = this.serial.BytesAvailable;
            try {
                Packet packet = Packet.ReadPacket(this.serial.Input);
                this.OnPacketReceived(packet);
            }
            catch (Exception e) {
                Console.WriteLine($"Failed to create packet. Start buffer = {buffer}, End buffer = {this.serial.BytesAvailable}", e);
            }
        }
    }
}
