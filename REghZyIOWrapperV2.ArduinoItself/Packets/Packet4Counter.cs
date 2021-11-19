using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.ArduinoItself.Packets {
    [PacketImplementation]
    public class Packet4Counter : PacketACK {
        public static int Counter { get; set; }

        public bool Increment;
        public int Count;

        public Packet4Counter(bool increment) {
            this.Increment = increment;
        }

        public Packet4Counter(int count) {
            this.Count = count;
        }

        static Packet4Counter() {
            RegisterACKPacket<Packet4Counter>(4, (d, k, i, l) => {
                return new Packet4Counter(i.ReadBool());
            }, (d, k, i, l) => {
                return new Packet4Counter((int) i.ReadInt());
            });
        }

        public override ushort GetLengthToServer() {
            return 1;
        }

        public override ushort GetLengthToClient() {
            return 4;
        }

        public override void WriteToServer(IDataOutput output) {
            output.WriteBoolean(this.Increment);
        }

        public override void WriteToClient(IDataOutput output) {
            if (this.Increment) {
                output.WriteInt((uint) ++Counter);
            }
            else {
                output.WriteInt((uint) --Counter);
            }
        }
    }
}
