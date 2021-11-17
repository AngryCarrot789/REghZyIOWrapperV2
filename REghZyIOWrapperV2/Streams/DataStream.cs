using System;
using System.IO;

namespace REghZyIOWrapperV2.Streams {
    /// <summary>
    /// A base object stream that is always open
    /// </summary>
    public abstract class DataStream : IDisposable {
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
        public abstract long BytesAvailable { get; }

        public DataStream(Stream stream) {
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

        /// <summary>
        /// Flushes the write buffer
        /// </summary>
        public void Flush() {
            this.stream.Flush();
        }

        /// <summary>
        /// Disposes the internal stream
        /// </summary>
        public void Dispose() {
            this.input.Stream = null;
            this.output.Stream = null;
            this.stream.Dispose();
        }
    }
}
