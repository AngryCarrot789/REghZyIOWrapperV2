namespace REghZyIOWrapperV2.Packeting.Handling {
    public enum Priority {
        /// <summary>
        /// This packet must be received first ALL OF THE TIME
        /// </summary>
        HIGHEST = 1,

        /// <summary>
        /// This packet must be received very soon after coming in
        /// </summary>
        HIGH = 2,

        /// <summary>
        /// Doesn't really matter. This is typically used for monitoring
        /// </summary>
        NORMAL = 3,
        
        /// <summary>
        /// Really doesn't matter at all
        /// </summary>
        LOW = 4,

        /// <summary>
        /// Same as low, but even lower
        /// </summary>
        LOWEST = 5
    }
}
