using System;
using System.IO.Ports;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Packeting.Listeners;
using REghZyIOWrapperV2.Packeting.Packets;
using REghZyIOWrapperV2.Utils;

namespace REghZyIOWrapperV2.Demo {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine($"Input serial port ({string.Join(", ", SerialPort.GetPortNames())})");
            string port = Console.ReadLine();

            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            SerialConnection connection = new SerialConnection(port);
            PacketHandler handler = new PacketHandler();
            PacketSystem system = new PacketSystem(connection, handler);
            handler.RegisterListener<Packet1LongData>((p) => {
                Console.WriteLine($"Received Packet1LongData: {p.Data}");
            });

            handler.RegisterListener<Packet2YourInfo>((p) => {
                Console.WriteLine($"Received Packet2YourInfo. Name = '{p.Name}', DOB = '{p.DateOfBirth}', Age = '{p.age}'");
            });

            system.Connection.Connect();
            system.SendPacket(new Packet1LongData(1));
            system.SendPacket(new Packet1LongData(2));
            system.SendPacket(new Packet1LongData(3));
            system.SendPacket(new Packet1LongData(4));
            system.SendPacket(new Packet1LongData(5));
            system.SendPacket(new Packet1LongData(6));
            system.SendPacket(new Packet1LongData(7));
            system.SendPacket(new Packet1LongData(8));
            system.SendPacket(new Packet1LongData(9));
            system.SendPacket(new Packet1LongData(10));
            system.SendPacket(new Packet1LongData(11));
            system.SendPacket(new Packet1LongData(12));
            system.SendPacket(new Packet1LongData(5425));

            while (true) {
                system.ReadNextPacket();
                Console.WriteLine("What's your name?");
                string name = Console.ReadLine();
                Console.WriteLine("What's your DOB?");
                string dot = Console.ReadLine();
                Console.WriteLine("What's your age?");
                if (int.TryParse(Console.ReadLine(), out int age)) {
                    Packet2YourInfo info = new Packet2YourInfo();
                    info.Name = name;
                    info.DateOfBirth = dot;
                    info.age = age;
                    system.SendPacket(info);
                    Console.WriteLine($"Sent packet. Size = {info.GetLength()}");
                }
                else {
                    Console.WriteLine("bad age!!! not an int");
                }
            }
        }
    }
}