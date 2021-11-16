using System;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Connections {
    /// <summary>
    /// A base class for all connections
    /// </summary>
    public abstract class BaseConnection : IDisposable {
        /// <summary>
        /// Whether this instance is being disposed or not
        /// </summary>
        protected bool notDisposing = true;

        /// <summary>
        /// The data stream this connection has open
        /// <para>
        /// This may be null if <see cref="IsConnected"/> returns false
        /// </para>
        /// </summary>
        public abstract DataStream Stream { get; }

        /// <summary>
        /// Indicates whether this connection is open or not. 
        /// This also indicates whether the input/output streams are available (they may be null if this is false)
        /// <para>
        /// Calling <see cref="Connect"/> should result in this being <see langword="true"/>
        /// </para>
        /// <para>
        /// Calling <see cref="Disconnect"/> should result in this being <see langword="false"/>
        /// </para>
        /// </summary>
        public abstract bool IsConnected { get; }

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

        public virtual void Dispose() {
            this.notDisposing = false;
        }
    }
}
