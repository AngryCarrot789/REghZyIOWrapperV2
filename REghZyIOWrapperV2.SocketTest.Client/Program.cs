using System;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Arduino.ACK;
using REghZyIOWrapperV2.Arduino.Packets;
using REghZyIOWrapperV2.ArduinoItself.Packets;
using REghZyIOWrapperV2.Connections.Networking;
using REghZyIOWrapperV2.Packeting;
using REghZyIOWrapperV2.Packeting.ACK;
using REghZyIOWrapperV2.Packeting.Handling;

namespace REghZyIOWrapperV2.SocketTest.Client {
    class Program {
        static void Main(string[] args) {
            foreach (Type type in Packet.FindPacketImplementations()) {
                Packet.RunPacketCtor(type);
            }

            // this is my computer running
            Console.WriteLine($"localhost = {IPAddress.Loopback.ToString()}");
            Console.WriteLine("Connecting to server on localhost@1440");
            ThreadPacketSystem system = SocketConnector.MakeConnectionToServer(new IPEndPoint(IPAddress.Loopback, 1440));

            ACKProcessor<Packet3HardwareInfo> processor = new ACKProcessor3HardwareInfo(system, (info) => {
                switch (info) {
                    case Packet3HardwareInfo.HardwareInfos.HardwareName:
                        return "The client!";
                    case Packet3HardwareInfo.HardwareInfos.SerialPortName:
                        return IPAddress.Loopback.ToString();
                    default:
                        return "UNKNOWN_CODE";
                }
            });
            
            system.RegisterListener<Packet1DigitalWrite>((p) => {
                Console.WriteLine($"Received P1DW. Pin = {p.Pin}, State = {(p.State ? "HIGH" : "LOW")}");
            }, Priority.HIGHEST);
            
            system.RegisterHandler<Packet2SayHi>((p) => {
                Console.WriteLine($"Arduino said: '{p.Message}'!");
                return true;
            });
            
            system.RegisterListener<Packet3HardwareInfo>((p) => {
                Console.WriteLine($"Received P3HI. Dest = {p.Destination}, Key = {p.Key}, Code = {p.Code}, Info = {p.Information}");
            }, Priority.HIGHEST);

            system.StartThreads();

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
            
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Getting connection name...");
            Packet3HardwareInfo packet = new Packet3HardwareInfo(Packet3HardwareInfo.HardwareInfos.HardwareName);
            Console.WriteLine("Sending P3Hi.. len = " + (packet.GetLength() + 3));
            Task<Packet3HardwareInfo> packet1 = processor.MakeRequestAsync(packet);
            Console.WriteLine($"Name = {packet1.Result.Information}");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Getting COM port...");
            Task<Packet3HardwareInfo> packet2 = processor.MakeRequestAsync(new Packet3HardwareInfo(Packet3HardwareInfo.HardwareInfos.SerialPortName));
            Console.WriteLine($"Port = {packet2.Result.Information}");
            Console.WriteLine("----------------------------------------------------");
            
            Console.WriteLine($"Saying hi...");
            system.EnqueuePacket(new Packet2SayHi());
            
            int pin = 13;
            while (true) {
                Console.WriteLine($"Set pin {pin} to HIGH");
                system.EnqueuePacket(new Packet1DigitalWrite(pin, true));
                Thread.Sleep(1000);
                Console.WriteLine($"Set pin {pin} to LOW");
                system.EnqueuePacket(new Packet1DigitalWrite(pin, false));
            
            
                if (pin == 0) {
                    Console.WriteLine("Finished writing!");
                    return;
                }
            
                Thread.Sleep(1000);
            }
        }
    }
}
