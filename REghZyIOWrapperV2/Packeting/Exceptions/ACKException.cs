using System;
using System.Runtime.Serialization;

namespace REghZyIOWrapperV2.Packeting.Exceptions {
    public class ACKException : PacketException {
        public ACKException() {
        }

        public ACKException(string message) : base(message) {
        }

        public ACKException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ACKException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
