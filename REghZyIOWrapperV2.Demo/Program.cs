using System;
using System.IO.Ports;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.Listeners;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Demo {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine($"Input serial port ({string.Join(", ", SerialPort.GetPortNames())})");
            string port = Console.ReadLine();

            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            PacketSystem system = new SerialPacketSystem(port, 9600);
            system.RegisterListener(new GenericPacketListener<Packet1LongData>((p) => {
                Console.WriteLine($"Received Packet1LongData: {p.Data}");
            }));

            system.RegisterListener(new GenericPacketListener<Packet2YourInfo>((p) => {
                Console.WriteLine($"Received Packet2YourInfo. Name = '{p.Name}', DOB = '{p.DateOfBirth}', Age = '{p.age}'");
            }));

            system.Connection.Connect();

            while (true) {
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
                    Console.WriteLine($"Sent packet. Size = {9 + name.Length + dot.Length}");
                }
                else {
                    Console.WriteLine("bad age!!! not an int");
                }
            }
        }
    }
}