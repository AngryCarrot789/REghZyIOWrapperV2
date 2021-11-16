namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// The server creates <see cref="ToClient"/> and sends it to the client
    /// <para>
    /// The client receives the raw packet data, and constructs an acknowledgement (<see cref="ClientACK"/>) 
    /// packet which it processes internally (ProcessPacketToClientACK) and then creates a packet with 
    /// <see cref="ToServer"/>, fills in the relevent information, and sends it back to the server
    /// </para>
    /// <para>
    /// The server receives that and processes it (ProcessPacketToServer)
    /// </para>
    /// </summary>
    public enum DestinationCode {
        /// <summary>
        /// This packet was sent from the server to the client and the client receives it. Usually, a client-side packet instance won't contain this destination code
        /// </summary>
        ToClient = 1,

        /// <summary>
        /// This packet was sent from the server and received by the client, and the client is processing the packet instance
        /// </summary>
        ClientACK = 2,

        /// <summary>
        /// This packet was sent from the client to the server, and is processed by the server
        /// </summary>
        ToServer = 3,
    }
}
