using System;
using System.Net;
using System.Net.Sockets;
using REghZyIOWrapperV2.Connections.Networking;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Connections.Socketing {
    /// <summary>
    /// A reusable client connection. This will wait until the server has 
    /// accepted a socket connection, and then allowing data to be transceived
    /// </summary>
    public class SocketToServerConnection : BaseConnection {
        private readonly Socket server;
        private readonly EndPoint endPoint;
        private NetworkDataStream stream;
        private bool isConnected;

        /// <summary>
        /// The data stream which is linked to the server
        /// </summary>
        public override DataStream Stream => this.stream;

        /// <summary>
        /// Whether this client is connected to the server
        /// </summary>
        public override bool IsConnected => this.isConnected;

        /// <summary>
        /// The socket which links to the server
        /// </summary>
        public Socket Server => this.server;

        public EndPoint LocalEndPoint => this.server.LocalEndPoint;

        public EndPoint RemoteEndPoint {
            get {
                if (this.server.Connected) {
                    return this.server.RemoteEndPoint;
                }
                else {
                    return this.endPoint;
                }
            }
        }

        public SocketToServerConnection(EndPoint endPoint, SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp) {
            this.server = new Socket(endPoint.AddressFamily, socketType, protocol);
            this.server.SendTimeout = 30000;
            this.server.ReceiveTimeout = 30000;
            this.endPoint = endPoint;
        }

        public override void Connect() {
            if (this.isDisposed) {
                throw new Exception("Cannot connect once the instance has been disposed!");
            }

            if (this.isConnected) {
                throw new Exception("Already connected!");
            }

            // this.server.ConnectWithTimeout(this.endPoint);
            try {
                this.server.Connect(this.endPoint);
            }
            catch(Exception e) {
                this.server.Close();
                throw new Exception($"Failed to connect to {this.endPoint}", e);
            }

            this.stream = new NetworkDataStream(this.server);
            this.isConnected = true;
        }

        public override void Disconnect() {
            if (this.isDisposed) {
                throw new Exception("Cannot disconnect once the instance has been disposed!");
            }

            if (!this.isConnected) {
                throw new Exception("Already disconnected!");
            }

            this.server.Disconnect(true);
            this.stream.Dispose();
            this.stream = null;
            this.isConnected = false;
        }
    }
}
