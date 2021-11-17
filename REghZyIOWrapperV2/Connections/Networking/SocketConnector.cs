using System;
using System.Net;
using System.Net.Sockets;
using REghZyIOWrapperV2.Connections.Socketing;
using REghZyIOWrapperV2.Packeting;

namespace REghZyIOWrapperV2.Connections.Networking {
    /// <summary>
    /// Set the scene: My computer, and an arduino. Even though an arduino can't run C# code... still
    /// <para>
    /// The arduino is the server, and it invokes <see cref="AcceptClientConnection(Socket)"/>. This will wait
    /// until my computer has tried to connect to it, and once it has, it begins sending/receiving packets
    /// </para>
    /// <para>
    /// My computer makes a connection to the arduino by calling <see cref="MakeConnectionToServer(EndPoint)"/>. It sits
    /// there and waits until the arduino accepts it (it will call <see cref="AcceptClientConnection(Socket)"/>). And
    /// then it begins sending/receiving packets
    /// </para>
    /// <para>
    /// The reason the arduino is the server, is because it just is because i want it to be :-) and its easier
    /// </para>
    /// <para>
    /// Whereas, my computer, it only needs that arduino connection, and no other connections
    /// </para>
    /// </summary>
    public static class SocketConnector {
        public static Socket CreateServerSocket(EndPoint endPoint, AddressFamily addressFamily, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp) {
            Socket socket = new Socket(addressFamily, socketType, protocolType);
            socket.Bind(endPoint);
            // socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.UseLoopback, true);
            return socket;
        }

        public static void ConnectWithTimeout(this Socket socket, EndPoint endPoint, int timeoutMillis = -1) {
            IAsyncResult result = socket.BeginConnect(endPoint, null, null);
            result.AsyncWaitHandle.WaitOne(timeoutMillis, true);
            if (socket.Connected) {
                socket.EndConnect(result);
            }
            else {
                socket.Close();
                throw new Exception("Failed to connect server, took longer than " + timeoutMillis + " to connect");
            }
        }

        /// <summary>
        /// We are the client, and we want to wait for a connection to the server
        /// <para>
        /// You don't need to call <see cref="BaseConnection.Connect"/>, it will be done automatically in this method
        /// </para>
        /// </summary>
        /// <returns>
        /// A packet system that is already connected (it can be disconnected and connected again at will)
        /// </returns>
        public static ThreadPacketSystem MakeConnectionToServer(EndPoint serverEndPoint, long timeoutMillis = 10000) { // 192.168.1.195, being the arduino
            SocketToServerConnection connection = new SocketToServerConnection(serverEndPoint);
            connection.Connect();
            return new ThreadPacketSystem(connection);
        }

        /// <summary>
        /// We are the server, and we want to accept any incomming connection from clients
        /// <para>
        /// You don't need to call <see cref="BaseConnection.Connect"/>, it won't do anything.
        /// See <see cref="SocketToClientConnection"/>, it is a one-time connection, you must create a
        /// new instance to have a new connection
        /// </para>
        /// </summary>
        /// <param name="server">The server connection</param>
        /// <returns>
        /// A packet system that is connected to client
        /// </returns>
        public static ThreadPacketSystem AcceptClientConnection(Socket server) { // The server's socket, aka arduino
            Socket client = server.Accept();
            return new ThreadPacketSystem(new SocketToClientConnection(client));
        }
    }
}
