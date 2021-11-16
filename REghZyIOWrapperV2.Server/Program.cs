using System;
using System.IO.Ports;
using System.Threading;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Server {
    class Program {
        static void Main(string[] args) {
            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            Console.WriteLine($"Input serial port ({string.Join(", ", SerialPort.GetPortNames())})");
            string port = Console.ReadLine();

            PacketSystem system = new PacketSystem(new SerialConnection(port), new PacketHandler());
            system.Handler.RegisterListener<Packet1LongData>((p) => {
                Console.WriteLine($"Received Packet1LongData: {p.Data}");
            });

            system.Handler.RegisterListener<Packet2YourInfo>((p) => {
                Console.WriteLine($"Received Packet2YourInfo. Name = '{p.Name}', DOB = '{p.DateOfBirth}', Age = '{p.age}'");
            });

            system.Handler.RegisterListener<Packet2YourInfo>((p) => {
                Console.WriteLine($"Received Packet2YourInfo. Name = '{p.Name}', DOB = '{p.DateOfBirth}', Age = '{p.age}'");
            });

            system.Connection.Connect();
            while (true) {
                if (!system.ReadNextPacket()) {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
