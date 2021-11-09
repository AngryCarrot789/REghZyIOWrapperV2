using System;
using System.IO.Ports;

namespace REghZyIOWrapperV2.PacketConnections {
    public class SerialDataConnection : DataConnection {
        private readonly SerialPort port;

        public SerialPort Port { get => this.port; }

        public bool IsConnected => this.port.IsOpen;

        public override int BytesAvailable => this.port.BytesToRead;


        public SerialDataConnection(SerialPort port) : base(port.BaseStream) {
            if (port == null) {
                throw new NullReferenceException("Port cannot be null");
            }

            this.port = port;
        }

        public override bool CanRead() {
            return this.port.BytesToRead > 0;
        }
    }
}
