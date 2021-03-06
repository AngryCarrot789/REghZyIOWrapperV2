using System;
using System.IO.Ports;
using System.Threading;
using REghZyIOWrapperV2.Arduino.ACK;
using REghZyIOWrapperV2.Arduino.Packets;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;

namespace REghZyIOWrapperV2.ArduinoItself {
    class Program {
        static void Main(string[] args) {
            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            Console.WriteLine($"Available ports: {string.Join(", ", SerialPort.GetPortNames())}");
            string port = Console.ReadLine();
            ThreadPacketSystem system = new ThreadPacketSystem(new SerialConnection(port));

            ACKProcessor<Packet3HardwareInfo> processor = new ACKProcessor3HardwareInfo(system, (info) => {
                switch (info) {
                    case Packet3HardwareInfo.HardwareInfos.HardwareName:
                        return "REghZyIOWrapperV2.Arduino.Program - The Arduino";
                    case Packet3HardwareInfo.HardwareInfos.SerialPortName:
                        return port;
                    default:
                        return "UNKNOWN_CODE";
                }
            });

            system.RegisterListener<Packet1DigitalWrite>((p) => {
                Console.WriteLine($"Received P1DW. Pin = {p.Pin}, State = {(p.State ? "HIGH" : "LOW")}");
            }, Priority.HIGHEST);

            system.RegisterListener<Packet3HardwareInfo>((p) => {
                Console.WriteLine($"Received P3HI. Dest = {p.Destination}, Key = {p.Key}, Code = {p.Code}, Info = {p.Information}");
            }, Priority.HIGHEST);

            system.Start();

            // Thread thread = new Thread(() => {
            //     if (system.HandleNextPackets(5) == 0) {
            //         Thread.Sleep(1);
            //     }
            // });

            Console.WriteLine("----------------------------------------------------");
            // Console.WriteLine("----------------------------------------------------");
            // Console.WriteLine("Getting connection name...");
            // Task<Packet3HardwareInfo> packet1 = processor.ReceivePacketAsync(processor.SendRequest(new Packet3HardwareInfo(Packet3HardwareInfo.HardwareInfos.HardwareName)));
            // Console.WriteLine($"Name = {packet1.Result.Information}");
            // Console.WriteLine("----------------------------------------------------");
            // Console.WriteLine("Getting COM port...");
            // Task<Packet3HardwareInfo> packet2 = processor.ReceivePacketAsync(processor.SendRequest(new Packet3HardwareInfo(Packet3HardwareInfo.HardwareInfos.SerialPortName)));
            // Console.WriteLine($"Port = {packet2.Result.Information}");
            // Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Starting in 5 seconds...");
            Thread.Sleep(5000);
            Console.WriteLine("Polling packets...");
            while (true) {
                if (system.HandleReadPackets(5) == 0) {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
