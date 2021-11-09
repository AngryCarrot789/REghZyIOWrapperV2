using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REghZyIOWrapperV2.Connections {
    /// <summary>
    /// A connection that constantly is constantly checking if there's data available
    /// </summary>
    public abstract class ThreadConnection : BaseConnection {
        private int sleepTime;

        private readonly Thread thread;

        /// <summary>
        /// The amount of time the read thread will pause reading if there is no data left, or this connection is closed
        /// <para>
        /// The higher, the longer it takes to process more data, but also the more data that may be available for instant read
        /// </para>
        /// </summary>
        public int SleepTime { get => this.sleepTime; set => this.sleepTime = value; }

        /// <summary>
        /// The thread that this instance is using
        /// </summary>
        public Thread Thread { get => this.thread; }

        protected ThreadConnection(Stream stream, int sleepTime = 1) : base(stream) {
            this.sleepTime = sleepTime;
            this.thread = new Thread(this.ThreadMain);
            this.thread.Start();
        }

        protected ThreadConnection(int sleepTime = 1) {
            this.sleepTime = sleepTime;
            this.thread = new Thread(this.ThreadMain);
            this.thread.Start();
        }

        public abstract void HandleDataAvailable();

        private void ThreadMain() {
            while(this.notDisposing) {
                if (this.IsConnected) {
                    if (this.BytesAvailable > 0) {
                        this.HandleDataAvailable();
                    }
                }

                Thread.Sleep(this.sleepTime);
            }
        }
    }
}
