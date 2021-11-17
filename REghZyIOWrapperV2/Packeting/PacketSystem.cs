using System;
using System.Collections.Concurrent;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Packeting.Exceptions;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting {
    /// <summary>
    /// A packet system is what handles sending and receiving packets, and delivering received packets to listeners
    /// <para>
    /// At it's base level, it's just a wrapper for reading and writing packets from 
    /// a <see cref="DataStream"/>. It also contains a map of handlers and listeners too
    /// </para>
    /// </summary>
    public class PacketSystem : IPacketSystem {
        private readonly PriorityMap map;

        /// <summary>
        /// The packets that have been read/received from the connection, and are ready to be processed
        /// </summary>
        protected readonly ConcurrentQueue<Packet> readQueue;

        /// <summary>
        /// The packets that have been created, and are ready to be sent to the connection
        /// </summary>
        protected readonly ConcurrentQueue<Packet> sendQueue;

        /// <summary>
        /// The packets that have been read/received from the connection, and are ready to be processed
        /// </summary>
        public ConcurrentQueue<Packet> ReadQueue { get => this.readQueue; }

        /// <summary>
        /// The packets that have been created, and are ready to be sent to the connection
        /// </summary>
        public ConcurrentQueue<Packet> SendQueue { get => this.sendQueue; }

        /// <summary>
        /// The connection that this packet system uses
        /// </summary>
        public BaseConnection Connection { get; }

        public PacketSystem(BaseConnection connection) {
            if (connection == null) {
                throw new NullReferenceException("Connection cannot be null");
            }

            this.Connection = connection;
            this.readQueue = new ConcurrentQueue<Packet>();
            this.sendQueue = new ConcurrentQueue<Packet>();
            this.map = new PriorityMap();
        }

        /// <summary>
        /// Starts the packet system. By default, this just calls <see cref="BaseConnection.Connect"/>
        /// </summary>
        public virtual void Start() {
            this.Connection.Connect();
        }

        /// <summary>
        /// Stops the packet system. By default, this just calls <see cref="BaseConnection.Disconnect"/>
        /// </summary>
        public virtual void Stop() {
            this.Connection.Disconnect();
        }

        public void RegisterHandler(GeneralHandler handler, Priority priority = Priority.NORMAL) {
            this.map.GetHandlers(priority).Add(handler);
        }

        public void RegisterHandler(Predicate<Packet> handler, Priority priority = Priority.NORMAL) {
            this.map.GetHandlers(priority).Add(new GeneralHandler(handler));
        }        
        
        public void RegisterHandler(Predicate<Packet> handler, Predicate<Packet> canProcess, Priority priority = Priority.NORMAL) {
            this.map.GetHandlers(priority).Add(new GeneralHandler(handler, canProcess));
        }

        public void RegisterHandler<T>(Predicate<T> handler, Priority priority = Priority.NORMAL) where T : Packet {
            this.map.GetHandlers(priority).Add(new GenericHandler<T>(handler));
        }

        public void RegisterListener(GeneralListener handler, Priority priority = Priority.NORMAL) {
            this.map.GetListeners(priority).Add(handler);
        }

        public void RegisterListener(Action<Packet> handler, Priority priority = Priority.NORMAL) {
            this.map.GetListeners(priority).Add(new GeneralListener(handler));
        }

        public void RegisterListener<T>(Action<T> handler, Priority priority = Priority.NORMAL) where T : Packet {
            this.map.GetListeners(priority).Add(new GenericListener<T>(handler));
        }

        /// <summary>
        /// Reads the next available packet, and enqueues it in <see cref="ReadQueue"/>
        /// </summary>
        /// <returns>
        /// True if a packet was read, otherwise false (if there wasn't enough data available to read a packet header)
        /// </returns>
        public bool ReadNextPacket() {
            long buffer = this.Connection.Stream.BytesAvailable;
            if (buffer < 1) {
                return false;
            }

            Packet packet;
            try {
                packet = Packet.ReadPacketHeadAndTail(this.Connection.Stream);
            }
            catch (PacketException e) {
                throw e;
            }
            catch (Exception e) {
                throw new PacketException($"Failed to read packet. Buffer before: {buffer}, After: {this.Connection.Stream.BytesAvailable}", e);
            }

            this.readQueue.Enqueue(packet);
            return true;
        }

        /// <summary>
        /// Writes the given number of packets queued in <see cref="SendQueue"/>
        /// </summary>
        /// <param name="count">The number of packets to try and write</param>
        /// <returns>
        /// The number of packets that were handled. This may not be equal to the given number of packets
        /// </returns>
        public int WriteNextPackets(int count = 5) {
            if (this.sendQueue.Count == 0) {
                return 0;
            }

            int sent = 0;
            IDataOutput output = this.Connection.Stream.Output;
            while (sent != count) {
                if (this.sendQueue.TryDequeue(out Packet packet)) {
                    sent++;
                    try {
                        Packet.WritePacket(packet, output);
                    }
                    catch (Exception e) {
                        throw new PacketException($"Failed to write packet {sent} of {count}, with ID {Packet.GetPacketID(packet)} of type {packet.GetType().Name}", e);
                    }
                }
                else {
                    return sent;
                }
            }

            return sent;
        }

        /// <summary>
        /// Handles the given number of packets that are currently queued in <see cref="ReadQueue"/>
        /// </summary>
        /// <param name="count">The number of packets to try and handle</param>
        /// <returns>
        /// The number of packets that were handled. This may not be equal to the given number of packets
        /// </returns>
        public int HandleReadPackets(int count = 10) {
            if (this.readQueue.Count == 0) {
                return 0;
            }

            int handled = 0;
            PriorityMap handler = this.map;
            while (handled != count) {
                if (this.readQueue.TryDequeue(out Packet packet)) {
                    handled++;
                    try {
                        handler.DeliverPacket(packet);
                    }
                    catch (Exception e) {
                        throw new Exception($"Failed to handle packet {handled}/{count}, with ID {Packet.GetPacketID(packet)} of type {packet.GetType().Name}", e);
                    }
                }
                else {
                    return handled;
                }
            }

            return handled;
        }

        /// <summary>
        /// Queues a packet to be sent (adds it to <see cref="SendQueue"/>)
        /// </summary>
        /// <param name="packet"></param>
        public void EnqueuePacket(Packet packet) {
            this.sendQueue.Enqueue(packet);
        }

        /// <summary>
        /// Immidiately sends a packet to the connection in this packet system. 
        /// This simply calls <see cref="Packet.WritePacket(Packet, Streams.IDataOutput)"/>
        /// <para>
        /// This method is blocking; you won't be able to do anything until ALL of the bytes have been written
        /// </para>
        /// </summary>
        /// <param name="packet">The packet to send (non-null)</param>
        public void SendPacketImmidiately(Packet packet) {
            Packet.WritePacket(packet, this.Connection.Stream.Output);
        }
    }
}