using System;
using System.Runtime.Serialization;
using REghZyIOWrapperV2.Packeting.ACK;

namespace REghZyIOWrapperV2.Packeting.Exceptions {
    public class OutOfIdsException : Exception {
        public OutOfIdsException() : this("There are no more available IDs available!") {

        }

        public OutOfIdsException(Type packetType) : base($"The packet type {packetType.Name} has used all of it's available IDs (all {PacketACK.IDEMPOTENCY_MAX} of them)") {

        }

        public OutOfIdsException(string message) : base(message) {

        }

        public OutOfIdsException(string message, Exception innerException) : base(message, innerException) {

        }

        protected OutOfIdsException(SerializationInfo info, StreamingContext context) : base(info, context) {

        }
    }
}
