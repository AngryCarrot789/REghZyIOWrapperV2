using System;
using System.IO;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Packeting.Packets;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.PacketConnections {
    /// <summary>
    /// A base class for an open connection (must be an active connection)
    /// </summary>
    public abstract class DataConnection {
        private readonly BlockingStream stream;
        private readonly DataInputStream input;
        private readonly DataOutputOutput output;

        /// <summary>
        /// The actual stream that this connection is built on
        /// </summary>
        public BlockingStream Stream {
            get => this.stream; 
        }

        /// <summary>
        /// The data input stream (for reading)
        /// </summary>
        public DataInputStream Input {
            get => this.input;
        }

        /// <summary>
        /// The data output stream (for writing)
        /// </summary>
        public DataOutputOutput Output {
            get => this.output;
        }

        /// <summary>
        /// Gets the number of bytes that can be read without blocking
        /// </summary>
        public abstract int BytesAvailable { get; }

        public DataConnection(Stream stream) {
            if (stream == null) {
                throw new NullReferenceException("Stream cannot be null");
            }

            this.stream = new BlockingStream(stream);
            this.input = new DataInputStream(this.stream);
            this.output = new DataOutputOutput(this.stream);
        }

        /// <summary>
        /// Whether there are any bytes in the input stream
        /// </summary>
        /// <returns></returns>
        public abstract bool CanRead();

        public Packet ReadPacket() {
            byte id = this.input.ReadByte();
            return Packet.ReadPacket(id, this.input);
        }

        public void WritePacket(Packet packet) {
            Packet.WritePacket(packet, this.output);
        }

        /// <summary>
        /// Flushes the stream buffer
        /// </summary>
        public void Flush() {
            this.stream.Flush();
        }
    }
}
