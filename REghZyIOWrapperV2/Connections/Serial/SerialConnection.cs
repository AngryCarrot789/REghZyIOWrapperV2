using System;
using System.IO.Ports;
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
            this.port.ErrorReceived += this.Port_ErrorReceived;
            // this.port.ReceivedBytesThreshold = 4096;
            // this.port.Handshake = Handshake.None;
            // this.port.ReadTimeout = 10000;
            // this.port.WriteTimeout = 10000;
        }

        private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
            switch (e.EventType) {
                case SerialError.TXFull:
                    Console.WriteLine($"[SerialError] - TXFull: Write buffer was overridden");
                    break;
                case SerialError.RXOver:
                    Console.WriteLine($"[SerialError] - RXOver: Too much data received, cannot read quick enough!");
                    break;
                case SerialError.Overrun:
                    Console.WriteLine($"[SerialError] - Overrun: The last written character was overridden... packet loss!");
                    break;
                case SerialError.RXParity:
                    Console.WriteLine($"[SerialError] - RXParity: A parity error was detected!");
                    break;
                case SerialError.Frame:
                    Console.WriteLine($"[SerialError] - Frame: A framing error was detected!");
                    break;
                default:
                    break;
            }
        }

        public override void Connect() {
            this.port.Open();
            // this.port.DtrEnable = true;
            this.stream = new SerialDataStream(this.port);
        }

        public override void Disconnect() {
            // this.port.DtrEnable = false;
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
