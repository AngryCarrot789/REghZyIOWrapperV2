using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Packeting.Listeners;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Server {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine($"Input serial port ({string.Join(", ", SerialPort.GetPortNames())})");
            string port = Console.ReadLine();

            IdempotencyKeyStore store = new IdempotencyKeyStore();

            store.Put(1);
            store.Put(2);
            store.Put(3);
            store.Put(5);
            store.Put(6);

            store.Put(4);

            foreach (uint key in store.GetEnumerator()) {
                Console.WriteLine("Key: " + key);
            }

            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            PacketSystem system = new PacketSystem(new SerialConnection(port), new PacketHandler());
            system.Handler.RegisterListener<Packet1LongData>((p) => {
                Console.WriteLine($"Received Packet1LongData: {p.Data}");
            });

            system.Handler.RegisterListener<Packet2YourInfo>((p) => {
                Console.WriteLine($"Received Packet2YourInfo. Name = '{p.Name}', DOB = '{p.DateOfBirth}', Age = '{p.age}'");
            });

            system.Connection.Connect();
            while (true) {
                system.ReadNextPacket();
            }
        }
    }
}
