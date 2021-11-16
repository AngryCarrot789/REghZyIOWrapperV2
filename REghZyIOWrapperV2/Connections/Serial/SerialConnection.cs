using System.IO.Ports;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Connections.Serial {
    /// <summary>
    /// A wrapper for a serial port, containing a data stream
    /// </summary>
    public class SerialConnection : BaseConnection {
        private SerialDataStream stream;
        private readonly SerialPort port;

        public override DataStream Stream => this.stream;

        public override bool IsConnected => this.port.IsOpen;

        public SerialConnection(string port) {
            this.port = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
        }

        public override void Connect() {
            this.port.Open();
            this.stream = new SerialDataStream(this.port);
        }

        public override void Disconnect() {
            this.port.DiscardInBuffer();
            this.port.DiscardOutBuffer();
            this.port.Close();
            this.stream = null;
        }

        public override void Dispose() {
            base.Dispose();
            this.port.DiscardInBuffer();
            this.port.DiscardOutBuffer();
            this.port.Close();
            this.port.Dispose();
        }
    }
}
