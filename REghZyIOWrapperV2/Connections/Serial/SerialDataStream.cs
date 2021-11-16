using System.IO.Ports;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Connections.Serial {
    public class SerialDataStream : DataStream {
        private readonly SerialPort port;

        public SerialPort Port { get => this.port; }

        public override int BytesAvailable => this.port.BytesToRead;

        public SerialDataStream(SerialPort port) : base(port.BaseStream) {
            this.port = port;
        }

        public override bool CanRead() {
            return this.port.BytesToRead > 0;
        }
    }
}
