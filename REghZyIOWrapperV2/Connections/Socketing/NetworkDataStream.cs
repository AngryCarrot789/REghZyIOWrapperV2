using System.Net.Sockets;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Connections.Socketing {
    /// <summary>
    /// A data stream that uses a <see cref="NetworkStream"/> as an underlying stream for reading/writing data
    /// </summary>
    public class NetworkDataStream : DataStream {
        private readonly NetworkStream network;

        private readonly Socket socket;

        public override long BytesAvailable => this.socket.Available; //this.network.DataAvailable ? long.MaxValue : 0;

        public NetworkStream Network { get => this.network; }

        public Socket Socket { get => this.socket; }

        public NetworkDataStream(Socket socket) : base(new NetworkStream(socket)) {
            this.socket = socket;
            this.network = (NetworkStream) base.Stream.BaseStream;
        }

        public override bool CanRead() {
            return this.network.DataAvailable;
        }
    }
}
