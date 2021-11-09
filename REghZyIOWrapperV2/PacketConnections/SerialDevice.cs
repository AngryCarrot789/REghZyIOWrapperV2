using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REghZyIOWrapperV2.PacketConnections {
    public partial class SerialDevice {
        private readonly SerialPort port;

        private DataConnection connection;

        public SerialDevice(string port) {
            this.port = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
        }
    }
}
