namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// The server creates <see cref="ToClient"/> and sends it to the client
    /// <para>
    /// The client receives the raw packet data, and constructs an acknowledgement (<see cref="ClientACK"/>) 
    /// packet which it processes internally (<see cref="ACKProcessor{T}.OnProcessPacketToClientACK(T)"/>) 
    /// and then creates a packet with <see cref="ToServer"/>, fills in the relevent information, and sends it back to the server
    /// </para>
    /// <para>
    /// The server receives that and processes it (<see cref="ACKProcessor{T}.OnProcessPacketToServer(T)"/>)
    /// </para>
    /// </summary>
    public enum DestinationCode {
        /// <summary>
        /// This packet was sent from the server to the client and the client receives it. 
        /// Usually, a client-side packet instance won't contain this destination code
        /// <para>
        /// This means the packet hasn't gone anywere yet, the server is still processing, and will soon send it to them
        /// </para>
        /// </summary>
        ToClient = 1,

        /// <summary>
        /// This packet was sent from the server and received by the client, and the client is processing the packet instance
        /// <para>
        /// This means the packet has gone from us, to them, and they are processing it
        /// </para>
        /// </summary>
        ClientACK = 2,

        /// <summary>
        /// This packet was sent from the client to the server, and is processed by the server
        /// <para>
        /// This usually means, the packet has gone from us, to them, and back to us
        /// </para>
        /// </summary>
        ToServer = 3,
    }
}
