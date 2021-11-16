using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Packeting.Exceptions;
using REghZyIOWrapperV2.Packeting.Handling;

namespace REghZyIOWrapperV2.Packeting {
    /// <summary>
    /// A thread-based packet system that uses a read and write thread to enqueue packets that have been 
    /// read from the connection, and to also send packets to the connection
    /// <para>
    /// It does not poll any packets, that must be done manually, 
    /// maybe via a "ProcessPackets" method in an update loop on the main thread
    /// </para>
    /// </summary>
    public class ThreadPacketSystem : PacketSystem, IDisposable {
        private static int READ_THREAD_COUNT = 0;
        private static int SEND_THREAD_COUNT = 0;

        // it should be "writeThread"... but i just cant... look... they're both the same number of chars :')
        private readonly Thread readThread;
        private readonly Thread sendThread;

        private volatile bool shouldRun;
        private volatile bool canRead;
        private volatile bool canSend;

        private volatile int writeCount;

        private bool isPaused;

        private int readCount;
        private int sendCount;

        /// <summary>
        /// The number of packets that the write thread should try to send each time
        /// <para>
        /// See the comments on <see cref="PacketSystem.WriteNextPackets(int)"/>, this may not be the exact
        /// number of packets that get written every time. The ability to write more than 1 is only for extra speed... maybe
        /// </para>
        /// </summary>
        public int WriteCount {
            get => this.writeCount;
            set => this.writeCount = value;
        }

        /// <summary>
        /// Sets whether the read thread can run or not. If set to <see langword="false"/>, it will not stop 
        /// the thread, it will simply sit at idle until this becomes <see langword="true"/>
        /// </summary>
        public bool CanRead {
            get => this.canRead;
            set => this.canRead = value;
        }

        /// <summary>
        /// Sets whether the send/write thread can run or not. If set to <see langword="false"/>, it will not stop 
        /// the thread, it will simply sit at idle until this becomes <see langword="true"/>
        /// </summary>
        public bool CanSend {
            get => this.canSend;
            set => this.canSend = value;
        }

        /// <summary>
        /// The exact number of packets that have been read
        /// </summary>
        public int PacketsRead => this.readCount;

        /// <summary>
        /// The exact number of packets that have been sent
        /// </summary>
        public int PacketsSent => this.sendCount;

        /// <summary>
        /// The thread used to read packets
        /// </summary>
        public Thread ReadThread => this.readThread;

        /// <summary>
        /// The thread used to send packets
        /// </summary>
        public Thread SendThread => this.sendThread;

        /// <summary>
        /// Creates a new instance of the threaded packet system
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="handler"></param>
        /// <param name="writeCount"></param>
        public ThreadPacketSystem(BaseConnection connection, int writeCount = 3) : base(connection) {
            this.readThread = new Thread(this.ReadMain) {
                Name = $"REghZy Read Thread {++READ_THREAD_COUNT}"
            };

            this.sendThread = new Thread(this.WriteMain) {
                Name = $"REghZy Write Thread {++SEND_THREAD_COUNT}"
            };

            this.writeCount = writeCount;
        }

        /// <summary>
        /// Starts both the read and write threads
        /// </summary>
        public override void Start() {
            base.Start();
            this.shouldRun = true;
            this.canRead = true;
            this.canSend = true;
            this.readThread.Start();
            this.sendThread.Start();
        }

        /// <summary>
        /// Fully stops both the reader and writer threads. They cannot be restarted!
        /// </summary>
        public override void Stop() {
            this.shouldRun = false;
            this.canRead = false;
            this.canSend = false;
            this.readThread.Join();
            this.sendThread.Join();
            base.Stop();
        }

        private void ReadMain() {
            while (this.shouldRun) {
                try {
                    while (this.canRead) {
                        if (ReadNextPacket()) {
                            this.readCount++;
                            continue;
                        }
                        else {
                            Thread.Sleep(1);
                        }
                    }
                }
                catch(PacketException e) {
                    // Console.WriteLine("Failed to read packet!");
                    // Exception ex = e;
                    // while(ex != null) {
                    //     Console.WriteLine(ex.Message);
                    //     Console.WriteLine(ex.StackTrace);
                    //     ex = ex.InnerException;
                    // }

                    throw e;
                }

                // A big wait time, because it's very unlikely that the ability to read packets
                // will be changed in a very tight time window
                Thread.Sleep(500);
            }
        }

        private void WriteMain() {
            while (this.shouldRun) {
                try {
                    while (this.canSend) {
                        int write = WriteNextPackets(this.writeCount);
                        if (write == 0) {
                            Thread.Sleep(1);
                            continue;
                        }

                        this.sendCount += write;
                    }
                }
                catch (PacketException e) {
                    // Console.WriteLine("Failed to write packet!", e);
                    // Exception ex = e;
                    // while (ex != null) {
                    //     Console.WriteLine(ex.Message);
                    //     Console.WriteLine(ex.StackTrace);
                    //     ex = ex.InnerException;
                    // }
                    throw e;
                }

                // A big wait time, because it's very unlikely that the ability to
                // write packets will be changed in a very tight time window... that
                // could change though... maybe... not really
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Toggles whether reading and writing packets is paused
        /// </summary>
        /// <returns>
        /// Whether read/writing is now paused or not. 
        /// <see langword="false"/> means nothing can be read or written. 
        /// <see langword="true"/> means packets can be read and written
        /// </returns>
        public bool TogglePause() {
            if (this.isPaused) {
                this.canRead = true;
                this.canSend = true;
                this.isPaused = false;
                return false;
            }
            else {
                this.canRead = false;
                this.canSend = false;
                this.isPaused = true;
                return true;
            }
        }

        public void Dispose() {
            Stop();
        }
    }
}
