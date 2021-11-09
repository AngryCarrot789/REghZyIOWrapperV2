using System;
using System.IO.Ports;

namespace REghZyIOWrapperV2.Connections {
    public class SerialConnection : BaseConnection {
        private readonly SerialPort port;
        private Action onDataAvailable;

        public Action OnDataAvailable {
            get => this.onDataAvailable; 
            set => this.onDataAvailable = value;
        }

        public override int BytesAvailable => this.port.BytesToRead;

        public override bool IsConnected => this.port.IsOpen;

        public SerialConnection(string port, int baudRate = 9600, int sleepTime = 10, Action onDataAvailable = null) {
            this.port = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);
            this.port.DataReceived += this.Port_DataReceived;
            this.onDataAvailable = onDataAvailable;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            this.onDataAvailable?.Invoke();
        }

        public override void Connect() {
            this.port.Open();
            InitStreams(this.port.BaseStream);
        }

        public override void Disconnect() {
            this.port.Close();
        }

        public override void Dispose() {
            this.port.Dispose();
            base.Dispose();
        }
    }
}
