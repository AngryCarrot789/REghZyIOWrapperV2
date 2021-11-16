using System;
using System.Runtime.Serialization;

namespace REghZyIOWrapperV2.Packeting.Exceptions {
    [Serializable]
    public class PacketCreationFailure : PacketException {
        public PacketCreationFailure() { }
        public PacketCreationFailure(string message) : base(message) { }
        public PacketCreationFailure(string message, Exception inner) : base(message, inner) { }
        protected PacketCreationFailure(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
