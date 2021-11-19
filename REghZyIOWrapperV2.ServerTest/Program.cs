using System;
using System.IO.Ports;
using System.Threading;
using REghZyIOWrapperV2.Arduino.ACK;
using REghZyIOWrapperV2.Arduino.Packets;
using REghZyIOWrapperV2.ArduinoItself.ACK;
using REghZyIOWrapperV2.ArduinoItself.Packets;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Utils;

namespace REghZyIOWrapperV2.ServerTest {
    class Program {
        static void Main(string[] args) {
            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            Console.WriteLine($"Available ports: {string.Join(", ", SerialPort.GetPortNames())}");

            string port = Console.ReadLine();
            SerialConnection connection = new SerialConnection(port);
            PacketSystem system = new ThreadPacketSystem(connection);
            ACKProcessor<Packet3HardwareInfo> processor = new ACKProcessor3HardwareInfo(system, (info) => {
                switch (info) {
                    case Packet3HardwareInfo.HardwareInfos.HardwareName:
                        return "Hello hardware name xd";
                    case Packet3HardwareInfo.HardwareInfos.SerialPortName:
                        return port;
                    default:
                        return "UNKNOWN_CODE";
                }
            });

            ACKProcessor4Counter proc1 = new ACKProcessor4Counter(system);

            system.RegisterListener<Packet1DigitalWrite>((p) => {
                Logger.LogNameDated("RX", $"P1DW - Pin {p.Pin} | {(p.State ? "HIGH" : "LOW")}");
            }, Priority.HIGHEST);

            system.RegisterHandler<Packet2SayHi>((p) => {
                Logger.LogNameDated("RX", $"P2SH - They said {p.Message}!");
                return true;
            });

            system.RegisterListener<Packet3HardwareInfo>((p) => {
                Logger.LogNameDated("RX", $"P3HI - Dest = {p.Destination} | Key = {p.Key} | Code = {p.Code} | Info = {p.Information}");
            }, Priority.HIGHEST);

            system.Start();
            connection.ClearBuffers();

            Thread thread = new Thread(() => {
                while (true) {
                    if (system.HandleReadPackets(5) == 0) {
                        Thread.Sleep(1);
                    }
                }
            });

            thread.Start();

            Console.WriteLine("Starting in 5 seconds...");
            Thread.Sleep(5000);

            // Console.WriteLine("----------------------------------------------------");
            // Console.WriteLine("Getting connection name...");
            // Packet3HardwareInfo packet = new Packet3HardwareInfo(Packet3HardwareInfo.HardwareInfos.HardwareName);
            // Logger.LogNameDated("TX", $"Sending (Length = {packet.GetLength() + 3})");
            // Packet3HardwareInfo p3hi = processor.MakeRequestAsync(packet).Result;
            // Console.WriteLine($"Name = {p3hi.Information}");
            // Console.WriteLine("----------------------------------------------------");

            Packet4Counter incr;
            Console.WriteLine("----------------------------------------------------");
            Logger.LogNameDated("TX", $"Sending increment...");
            incr = proc1.MakeRequestAsync(new Packet4Counter(true)).Result;
            Logger.LogNameDated("TX", $"Value = {incr.Count}");
            Logger.LogNameDated("TX", $"Sending increment...");
            incr = proc1.MakeRequestAsync(new Packet4Counter(true)).Result;
            Logger.LogNameDated("TX", $"Value = {incr.Count}");
            Logger.LogNameDated("TX", $"Sending increment...");
            incr = proc1.MakeRequestAsync(new Packet4Counter(false)).Result;
            Logger.LogNameDated("TX", $"Value = {incr.Count}");
            Console.WriteLine("----------------------------------------------------");

            //Console.WriteLine("Getting COM port...");
            //Task<Packet3HardwareInfo> packet2 = processor.MakeRequestAsync(new Packet3HardwareInfo(Packet3HardwareInfo.HardwareInfos.SerialPortName));
            //Console.WriteLine($"Port = {packet2.Result.Information}");
            //Console.WriteLine("----------------------------------------------------");

            // Console.WriteLine($"Saying hi...");
            // system.EnqueuePacket(new Packet2SayHi());

            int pin = 2;
            while (true) {
                // Logger.LogNameDated("TX", $"Set pin {pin} to HIGH");
                // system.EnqueuePacket(new Packet1DigitalWrite(pin, true));
                // Thread.Sleep(500);
                // Logger.LogNameDated("TX", $"Set pin {pin--} to LOW");
                // system.EnqueuePacket(new Packet1DigitalWrite(pin, false));
                // 
                // 
                // if (pin == 0) {
                //     Console.WriteLine("Finished writing!");
                //     return;
                // }

                Thread.Sleep(500);
            }
        }
    }
}
