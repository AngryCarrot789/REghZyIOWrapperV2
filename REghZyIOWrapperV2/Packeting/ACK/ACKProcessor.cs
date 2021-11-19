using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Packeting.Exceptions;
using REghZyIOWrapperV2.Packeting.Handling;
using REghZyIOWrapperV2.Utils;

namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// A helper class for managing ACK packets (aka packets that can be sent, and then receive the same packet (but with extra "details") back)
    /// <para>
    /// The client sends a request to the server, the server receives that request and processes/acknowledges it, and then sends a responce back.
    /// The responce uses the same idempotency key that the client used, meaning the keys are generated client side
    /// </para>
    /// <para>
    /// The perspective of the server is the one that receives requests, and sends responces back. The perspective
    /// of the client is the one that makes requests and receives responces
    /// </para>
    /// </summary>
    /// <typeparam name="TPacketACK">The type of ACK packet</typeparam>
    public abstract class ACKProcessor<TPacketACK> where TPacketACK : PacketACK {
        /// <summary>
        /// Maps the idempoency key to the originally sent ACK packet
        /// <para>
        /// This is used to cache packets that the server has created, so that if the client 
        /// </para>
        /// </summary>
        private readonly Dictionary<uint, TPacketACK> sendPacketCache = new Dictionary<uint, TPacketACK>();

        /// <summary>
        /// Maps the idempotency key to the last time a packet (with the key) was sent. This is
        /// used to auto-remove idempotency keys, if it's been unused for a specific time
        /// </summary>
        private readonly Dictionary<uint, long> lastSendTimes = new Dictionary<uint, long>();

        /// <summary>
        /// Maps the idempotency key to a packet. When ACK packets of type <see cref="TPacketACK"/> are received,
        /// they are shoved in here, and then they can be fetched from the <see cref="ReceivePacketAsync(uint)"/> method 
        /// </summary>
        private readonly Dictionary<uint, TPacketACK> readPacketCache = new Dictionary<uint, TPacketACK>();

        protected readonly PacketSystem packetSystem;

        protected bool isRequestUnderWay;

        private bool resendPacket;
        private volatile int packetResendTime;
        private bool discardRepeatIdempotency;
        private bool removeUnusedIdempotency;
        private long removeIdempotencyTime;
        private long ignoreRepeatTime;
        private long cacheExpiryTime;
        private bool useCache;
        private bool reprocessRepeatIdempotency;

        /// <summary>
        /// A value indicating whether extra packets are allowed to be sent if the ACK packets are not received within the timeout
        /// <para>
        /// This is only used client-side. The server doesn't resend packets, but it will if the client requests the same packet
        /// </para>
        /// </summary>
        public bool ResendPacket {
            get => this.resendPacket;
            set => this.resendPacket = value;
        }

        /// <summary>
        /// The amount of time to wait before sending the same packet again, if the responce is not received
        /// <para>
        /// This is only used if <see cref="ResendPacket"/> is set to <see langword="true"/>
        /// </para>
        /// </summary>
        public int PacketResendTime {
            get => this.packetResendTime;
            set => this.packetResendTime = value;
        }

        /// <summary>
        /// States whether to fully ignore packets that use an idempotency key that was already handled
        /// <para>
        /// Repeated packets are usually encountered if the client sends the exact same packet multiple times
        /// in order to increase the chances of the packet receiving it (if packet loss is a possibility)
        /// </para>
        /// <para>
        /// If this is set to <see langword="true"/>, <see cref="RemoveUnusedIdempotencyKeys"/>, <see cref="IdempotencyKeyTimeout"/>
        /// and <see cref="IgnoreRepeatTime"/> become useless
        /// </para>
        /// </summary>
        public bool DiscardRepeatedPackets {
            get => this.discardRepeatIdempotency;
            set => this.discardRepeatIdempotency = value;
        }

        /// <summary>
        /// States whether to remove any idempotency keys, if they haven't been used in a specific amount of time (see <see cref="IdempotencyKeyTimeout"/>)
        /// </summary>
        public bool RemoveUnusedIdempotencyKeys {
            get => this.removeUnusedIdempotency;
            set => this.removeUnusedIdempotency = value;
        }

        /// <summary>
        /// The amount of time that idempotency keys last after being used, in milliseconds
        /// <para>
        /// Note: the keys are not actively checked for this time, they are only checked when
        /// a packet (that uses an already existing idempotency key) is received
        /// </para>
        /// <para>
        /// Example: this value is 5000. A packet with key 1 is received, and then 7 seconds later, 
        /// another packet with key 1 is received. That packet will not encounter any problems, because
        /// key 1 will just be reused. But if it arrived less than 7 seconds later, if <see cref="RemoveUnusedIdempotencyKeys"/>
        /// is set to true, the packet will be discarded (not handled)
        /// </para>
        /// </summary>
        public long IdempotencyKeyTimeout {
            get => this.removeIdempotencyTime;
            set => this.removeIdempotencyTime = value;
        }

        /// <summary>
        /// The amount of time required between the same (aka duplicated) packets, in order to send back the cached packet
        /// <para>
        /// This can also be seen as an anti-packet-spam measure. If the same packets (with the same key, obviously) are received
        /// in less than this amount of time, then just ignore them
        /// </para>
        /// </summary>
        public long IgnoreRepeatTime {
            get => this.ignoreRepeatTime;
            set => this.ignoreRepeatTime = value;
        }

        /// <summary>
        /// States whether to use packet caching (in the event of repeated packets only)
        /// </summary>
        public bool UseCache {
            get => this.useCache;
            set => this.useCache = value;
        }

        /// <summary>
        /// The amount of time that packets in the sent cache will be removed 
        /// </summary>
        public long CacheExpiryTime {
            get => this.cacheExpiryTime;
            set => this.cacheExpiryTime = value;
        }

        /// <summary>
        /// States whether to simply ignore any repeated idempotency keys, and just process the request again
        /// <para>
        /// This essentially removes the entire use of idempotency keys
        /// </para>
        /// </summary>
        public bool AllowReprocessRepeatIdempotency {
            get => this.reprocessRepeatIdempotency;
            set => this.reprocessRepeatIdempotency = value;
        }

        /// <summary>
        /// The packet system that this ACK processor uses
        /// </summary>
        public PacketSystem PacketSystem {
            get => this.packetSystem;
        }

        protected ACKProcessor(PacketSystem packetSystem, Priority priority = Priority.HIGHEST) {
            if (packetSystem == null) {
                throw new ArgumentNullException(nameof(packetSystem), "Packet system cannot be null");
            }

            this.packetSystem = packetSystem;
            this.packetSystem.RegisterHandler<TPacketACK>(OnPacketReceived, priority);
            this.isRequestUnderWay = false;

            this.UseCache = true;
            this.CacheExpiryTime = 20000;
            this.ResendPacket = true;
            this.PacketResendTime = 1000;
            this.DiscardRepeatedPackets = false;
            this.RemoveUnusedIdempotencyKeys = true;
            this.IdempotencyKeyTimeout = 10000;
            this.IgnoreRepeatTime = 1000;
            this.AllowReprocessRepeatIdempotency = false;
        }

        /// <summary>
        /// Sets up the given packet's key and destination for you, and sends the packet, returning the ID of the packet
        /// <para>
        /// This packet is now participating in the ACK transaction, therefore, no other packets should be sent until
        /// the responce has been received (aka a packet, that this processor processes, 
        /// is received in direction <see cref="DestinationCode.ToClient"/>)
        /// </para>
        /// </summary>
        /// <param name="packet"></param>
        public uint SendRequest(TPacketACK packet) {
            if (this.isRequestUnderWay) {
                throw new Exception("Concurrent ACK request! A packet is already in transit");
            }

            this.isRequestUnderWay = true;
            packet.Key = PacketACK.GetNextID<TPacketACK>();
            packet.Destination = DestinationCode.ToServer;
            sendPacketCache[packet.Key] = packet;
            // lastSentTime = SystemMillis(); // don't need to add it here, because the client runs this
            this.packetSystem.EnqueuePacket(packet);
            this.packetSystem.EnqueuePacket(packet);
            this.packetSystem.EnqueuePacket(packet);
            return packet.Key;
        }

        /// <summary>
        /// A helper function for sending a packet back to the client. It automatically sets the key from <paramref name="fromClient"/>, 
        /// and sets the destination code to <see cref="DestinationCode.ToClient"/>.
        /// <para>
        /// This method just sets the key, and then invokes <see cref="SendBackFromACK(TPacketACK)"/>
        /// </para>
        /// <para>
        /// The packet "toServer" should contain all of the custom information 
        /// that you want to send to the client
        /// </para>
        /// </summary>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        protected void SendBackFromACK(TPacketACK fromClient, TPacketACK toClient) {
            toClient.Key = fromClient.Key;
            SendBackFromACK(toClient);
        }

        /// <summary>
        /// A helper function for sending a packet back to the client. It automatically sets the destination for you,
        /// and then adds the packet to the send packet cache (in case a request is made with the same ID),
        /// and then sends the given packet again (with the destination code of <see cref="DestinationCode.ToClient"/>)
        /// </summary>
        /// <param name="packet"></param>
        protected void SendBackFromACK(TPacketACK packet) {
            packet.Destination = DestinationCode.ToClient;
            this.sendPacketCache[packet.Key] = packet;
            this.packetSystem.EnqueuePacket(packet);
        }

        /// <summary>
        /// Called when the packet is received from the server or client
        /// <para>
        /// This is the client if the <see cref="PacketACK.Destination"/> is <see cref="DestinationCode.ServerACK"/>, and contains request information
        /// </para>
        /// <para>
        /// This is the server if the <see cref="PacketACK.Destination"/> is <see cref="DestinationCode.ToClient"/>, and contains client information
        /// </para>
        /// </summary>
        /// <param name="packet"></param>
        private bool OnPacketReceived(TPacketACK packet) {
            // acknowledgement
            if (packet.Destination == DestinationCode.ServerACK) {
                if (PacketACK.IsHandled(packet, true)) {
                    if (this.discardRepeatIdempotency) {
                        return true;
                    }

                    if (this.useCache) {
                        // this is a time limit, it means don't send the same packet back
                        // unless it's been a specific amount of time
                        long lastSendTime = this.lastSendTimes[packet.Key];
                        long systemMillis = SystemMillis();
                        if ((systemMillis - lastSendTime) > this.removeIdempotencyTime) {
                            if (removeUnusedIdempotency) {
                                // Remove old cached packets and idempotency keys
                                this.sendPacketCache.Remove(packet.Key);
                                this.lastSendTimes.Remove(packet.Key);
                                PacketACK.SetUnhandled(packet, true);
                            }
                            else {
                                return true;
                            }
                        }
                        else if ((systemMillis - lastSendTime) > this.ignoreRepeatTime) {
                            if (this.sendPacketCache.ContainsKey(packet.Key)) {
                                TPacketACK cached = this.sendPacketCache[packet.Key];
                                if (!cached.HasExpired()) {
                                    cached.Destination = DestinationCode.ToClient;
                                    this.packetSystem.EnqueuePacket(cached);
                                    Logger.LogNameDated("RX-ACK", $"Duplicated packet '{packet.GetType().Name}' with key '{packet.Key}'. Sending cached packet back");
                                    return true;
                                }
                            }
                            else {
                                Logger.LogNameDated("RX-ACK", $"Duplicated packet '{packet.GetType().Name}' with key '{packet.Key}' WITHOUT A CACHED PACKET!!!");
                                return true;
                            }
                        }
                    }

                    if (!this.reprocessRepeatIdempotency) {
                        return true;
                    }
                }

                if (OnProcessPacketToServerACK(packet)) {
                    this.lastSendTimes[packet.Key] = SystemMillis();
                    PacketACK.SetHandled(packet, true);
                    return true;
                }
                else {
                    return false;
                }
            }
            else if (packet.Destination == DestinationCode.ToClient) {
                this.isRequestUnderWay = false;
                // server has sent the data to client. The client should handle as
                if (PacketACK.IsHandled(packet, false)) {
                    // dont handle the packet again
                    Logger.LogNameDated("RX-ACK", $"Already handled '{packet.GetType().Name}' with key '{packet.Key}'");
                    return true;
                }
                else {
                    // packet contains actual good information!!! 
                    if (OnProcessPacketToClient(packet)) {
                        this.readPacketCache[packet.Key] = packet;
                        PacketACK.SetHandled(packet, false);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            else {
                // bug???
                throw new ACKException($"Received hardware info packet destination was not {DestinationCode.ServerACK} or {DestinationCode.ToClient}");
            }
        }

        /// <summary>
        /// Calling this will go through all cached packets, and remove any that 
        /// have been in there for too long (in order to save memory)
        /// </summary>
        public void UpdateCache() {
            long exipryTime = this.cacheExpiryTime;
            Dictionary<uint, long> timeCache = this.lastSendTimes;
            Dictionary<uint, TPacketACK> sendCache = this.sendPacketCache;
            List<uint> toRemoveKeys = new List<uint>(timeCache.Keys.Count);
            foreach (uint key in timeCache.Keys) {
                if (key > exipryTime) {
                    toRemoveKeys.Add(key);
                }
            }

            foreach (uint key in toRemoveKeys) {
                timeCache.Remove(key);
                sendCache.Remove(key);
            }
        }

        private static long SystemMillis() {
            // return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Waits until the last received packet is no longer null (meaning a new ACK packet has arrived), and then returns it as the task's result
        /// <para>
        /// This will wait forever until the packet has arrived
        /// </para>
        /// </summary>
        public async Task<TPacketACK> ReceivePacketAsync(uint id) {
            int readAttempts = 0;
            long start = SystemMillis();
            while (true) {
                if (this.readPacketCache.TryGetValue(id, out TPacketACK packet) && packet != null) {
                    // this.SendPacketCache.Remove(id);
                    this.readPacketCache.Remove(id);
                    return packet;
                }

                readAttempts++;
                // saves constantly calling SystemMills which could be slightly slow,
                // and also reduces the constant volatile read of packetResendTime
                if (readAttempts > 20) { 
                    readAttempts = 0;
                    if (this.resendPacket) {
                        long current = SystemMillis();
                        if ((current - start) > this.packetResendTime) {
                            start = current;
                            if (sendPacketCache.TryGetValue(id, out TPacketACK pkt)) {
                                Console.WriteLine($"Did not receive after {this.packetResendTime}ms... writing packet again");
                                this.packetSystem.EnqueuePacket(pkt);
                            }
                        }
                    }
                }

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Sends the given packet, and waits for a responce
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public async Task<TPacketACK> MakeRequestAsync(TPacketACK packet) {
            return await ReceivePacketAsync(SendRequest(packet));
        }

        /// <summary>
        /// This will only be called if this is the client. It is called when we (the client) 
        /// receive a packet from the server (usually after the server runs <see cref="OnProcessPacketToServerACK"/>)
        /// <para>
        /// If the client wanted data (which is usually the usage for ACK packets), 
        /// the packet in the parameters will usually contain the information requested
        /// </para>
        /// <para>
        /// Usually, this method is empty, because you usually use the <see cref="ReceivePacketAsync(uint)"/>
        /// method. But that method relies on <see cref="readPacketCache"/> containing the packet, and the packet is
        /// only placed in there once this method returns <see langword="true"/>
        /// </para>
        /// <para>
        /// And if this returns <see langword="false"/>, then <see cref="ReceivePacketAsync(uint)"/> will never return, the 
        /// idempotency key will NOT be stored, essentially meaning you completely ignore the packet (although listeners
        /// that use <see cref="Priority.HIGHEST"/> will be able to sniff it)
        /// </para>
        /// </summary>
        /// <returns>
        /// Whether the packet is actually useful and can be used/handled. 
        /// <see langword="true"/> means the idempotency key is stored, and anything that invoked <see cref="ReceivePacketAsync(uint)"/> will
        /// return successfully.
        /// <see langword="false"/> if the packet shouldn't be handled, and can possibly be sent to other handlers/listeners, 
        /// the idempotency key won't be stored, and <see cref="ReceivePacketAsync(uint)"/> will not return
        /// </returns>
        protected abstract bool OnProcessPacketToClient(TPacketACK packet);

        /// <summary>
        /// This will be called if we are the server. It is called when we receive a packet from the client, 
        /// aka the mid-way between getting and receiving data (that is, if the ACK packet is used for that)
        /// <para>
        /// If the client wanted data (which is usually the usage for ACK packets), the packet in the parameters  will usually 
        /// contain request information, which will be used to fill in data for a new packet, and then be sent to the client
        /// </para>
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the packet is fully handled, and should't be sent anywhere else (see <see cref="IHandler.Handle(Packet)"/>),
        /// <see langword="false"/> if the packet shouldn't be handled, and can possibly be sent to other handlers/listeners
        /// </returns>
        protected abstract bool OnProcessPacketToServerACK(TPacketACK packet);
    }
}
