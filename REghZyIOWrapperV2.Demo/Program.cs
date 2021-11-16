using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Packeting.Packets;

namespace REghZyIOWrapperV2.Demo {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine($"Input serial port ({string.Join(", ", SerialPort.GetPortNames())})");
            string port = Console.ReadLine();

            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            PacketSystem system = new PacketSystem(new SerialConnection(port), new PacketHandler());
            ACKProcessor3HardwareInfo infoProc = new ACKProcessor3HardwareInfo(system, (c) => {
                switch (c) {
                    case Packet3HardwareInfo.HardwareInfos.HardwareName:
                        return "Server Hardware";
                    case Packet3HardwareInfo.HardwareInfos.SerialPortName:
                        return port;
                    default:
                        return "<Error-Unknown_Code>";
                }
            });

            system.Handler.RegisterListener<Packet1LongData>((p) => {
                Console.WriteLine($"Received Packet1LongData: {p.Data}");
            }, Priority.HIGHEST);

            system.Handler.RegisterListener<Packet2YourInfo>((p) => {
                Console.WriteLine($"Received Packet2YourInfo. Name = '{p.Name}', DOB = '{p.DateOfBirth}', Age = '{p.age}'");
            }, Priority.HIGHEST);

            system.Handler.RegisterListener<Packet3HardwareInfo>((p) => {
                Console.WriteLine($"Received Packet3HardwareInfo. Key = {p.Key}, Dest = {p.Destination}, InfoCode = {p.Code}, Info = {p.Information}");
            }, Priority.HIGHEST);

            system.Connection.Connect();

            Thread thread = new Thread(() => {
                while (true) {
                    system.ReadNextPacket();
                }
            });

            thread.Start();

            Console.WriteLine("Getting server name...");
            Task<Packet3HardwareInfo> packet = infoProc.ReceivePacketAsync(infoProc.SendRequest(Packet3HardwareInfo.HardwareInfos.HardwareName));
            Console.WriteLine($"Name = {packet.Result.Information}");

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
                    Console.WriteLine($"Sent packet. Size = {info.GetLength()}");
                }
                else {
                    Console.WriteLine("bad age!!! not an int");
                }
            }
        }
    }
}