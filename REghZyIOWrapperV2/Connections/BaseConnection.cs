using System;
using System.IO;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Connections {
    /// <summary>
    /// A base class for all connections
    /// </summary>
    public abstract class BaseConnection : IDisposable {
        private BlockingStream stream;
        private DataInputStream input;
        private DataOutputOutput output;

        /// <summary>
        /// Whether this instance is being disposed or not
        /// </summary>
        protected bool notDisposing;

        /// <summary>
        /// The actual stream that this connection is built on
        /// </summary>
        public BlockingStream BaseStream {
            get => this.stream; 
            set => this.stream = value;
        }

        /// <summary>
        /// The data input stream (for reading)
        /// </summary>
        public DataInputStream Input {
            get => this.input;
            set => this.input = value;
        }

        /// <summary>
        /// The data output stream (for writing)
        /// </summary>
        public DataOutputOutput Output {
            get => this.output;
            set => this.output = value;
        }

        /// <summary>
        /// Gets the number of bytes that can be read without blocking
        /// </summary>
        public abstract int BytesAvailable { get; }

        /// <summary>
        /// Indicates whether this connection is open or not. 
        /// This also indicates whether the input/output streams are available (they will be null if this is false)
        /// <para>
        /// Calling <see cref="Connect"/> should result in this being <see langword="true"/>
        /// </para>
        /// <para>
        /// Calling <see cref="Disconnect"/> should result in this being <see langword="false"/>
        /// </para>
        /// </summary>
        public abstract bool IsConnected { get; }

        public BaseConnection(Stream stream) {
            InitStreams(stream);
        }

        public BaseConnection() {

        }

        /// <summary>
        /// Creates the connection, allowing data to be read and written
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Breaks the connection, stopping data from being read and written
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Disconnects and then connects
        /// </summary>
        public virtual void Restart() {
            Disconnect();
            Connect();
        }

        public void Flush() {
            this.stream.Flush();
        }

        public virtual void Dispose() {
            this.notDisposing = true;
            this.stream.Dispose();
        }

        protected void InitStreams(Stream stream) {
            if (stream == null) {
                throw new NullReferenceException("Stream cannot be null");
            }

            this.stream = new BlockingStream(stream);
            this.input = new DataInputStream(this.stream);
            this.output = new DataOutputOutput(this.stream);
        }
    }
}
