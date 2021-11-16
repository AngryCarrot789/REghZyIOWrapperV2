using System;
using System.IO.Ports;
using System.Threading;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
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
            ACKProcessor3HardwareInfo hardwareInfoProcessor = new ACKProcessor3HardwareInfo(system, (c) => {
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
            while (true) {
                if (!system.ReadNextPacket()) {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
