using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Arduino.ACK;
using REghZyIOWrapperV2.Arduino.Packets;
using REghZyIOWrapperV2.ArduinoItself.ACK;
using REghZyIOWrapperV2.ArduinoItself.Packets;
using REghZyIOWrapperV2.Connections.Serial;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Utils;

namespace thingyspooler {
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

            system.RegisterListener<Packet4Counter>((p) => {
                Logger.LogNameDated("RX", $"P4CC - Dest = {p.Destination} | Key = {p.Key} | Incr = {p.Increment} | Value = {p.Count}");
            }, Priority.HIGHEST);

            system.Start();
            connection.ClearBuffers();

            Console.WriteLine("Starting!");
            while(true) {
                if (system.HandleReadPackets(5) == 0) {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
