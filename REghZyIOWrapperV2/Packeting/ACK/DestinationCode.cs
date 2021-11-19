namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// The client creates <see cref="ToServer"/> and sends it to the server
    /// <para>
    /// The server receives the raw packet data (with the code <see cref="ToServer"/>), 
    /// and constructs an acknowledgement (<see cref="ServerACK"/>) packet which it processes 
    /// internally (<see cref="ACKProcessor{T}.OnProcessPacketToServerACK(T)"/>) 
    /// and then creates a packet with <see cref="ToClient"/>, fills in the relevent 
    /// information, and sends it back to the client
    /// </para>
    /// <para>
    /// The client receives that and processes it (<see cref="ACKProcessor{T}.OnProcessPacketToClient(T)"/>)
    /// </para>
    /// </summary>
    public enum DestinationCode : byte {
        /// <summary>
        /// This code should be use client side only. The server receives the
        /// ACK packet, and reads the raw bytes for the destination code
        /// </summary>
        ToServer = 0b0001,

        /// <summary>
        /// This packet was sent from the server to the client, and is being processed by the client
        /// </summary>
        ToClient = 0b0010,

        /// <summary>
        /// This packet was sent from the client and received by the server, and the server is processing the packet instance
        /// </summary>
        ServerACK = 0b0100,
    }
}
