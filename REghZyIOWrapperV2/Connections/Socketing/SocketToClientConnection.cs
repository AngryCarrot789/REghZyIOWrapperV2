using System;
using System.Net.Sockets;
using System.Net;
using REghZyIOWrapperV2.Streams;
using REghZyIOWrapperV2.Connections.Networking;

namespace REghZyIOWrapperV2.Connections.Socketing {
    /// <summary>
    /// Represents a one-time connection as a server. When this class is instantated, it is 
    /// assumed that the socket is already open. So calling <see cref="Connect"/> will do nothing
    /// <para>
    /// Calling <see cref="Disconnect"/> will fully disconenct and dispose of the socket, 
    /// meaning you cannot reconnect (it will throw an exception if you try to invoke <see cref="Connect"/>,
    /// just for the sake of bug tracking)
    /// </para>
    /// <para>
    /// An example of when this is used, is if "we" are an arduino (see <see cref="SocketConnector"/> for more info)
    /// </para>
    /// </summary>
    public class SocketToClientConnection : BaseConnection {
        private readonly Socket client;
        private readonly NetworkDataStream stream;

        /// <summary>
        /// The data stream which is linked to the server
        /// </summary>
        public override DataStream Stream => this.stream;

        /// <summary>
        /// Whether this client is connected to the server
        /// </summary>
        public override bool IsConnected => this.isDisposed;

        /// <summary>
        /// The socket that this connection is connected to
        /// </summary>
        public Socket Client => this.client;

        public EndPoint LocalEndPoint => this.client.LocalEndPoint;

        public EndPoint RemoteEndPoint => this.client.RemoteEndPoint;

        public SocketToClientConnection(Socket client) {
            this.client = client;
            this.stream = new NetworkDataStream(this.client);
        }

        public override void Connect() {
            if (this.isDisposed) {
                throw new Exception("Cannot reconnect once the instance has been disconnected!");
            }
        }

        public override void Disconnect() {
            if (this.isDisposed) {
                return;
            }

            this.client.Disconnect(false);
            this.stream.Dispose();
            base.Dispose();
        }
    }
}
