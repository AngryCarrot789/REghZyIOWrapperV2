using System;
using System.Threading;

namespace REghZyIOWrapperV2.Connections {
    /// <summary>
    /// A connection that constantly is constantly checking if there's data available, and raises an event when there is
    /// </summary>
    public abstract class BaseConnectionThreaded : BaseConnection {
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

        public delegate void DataAvailable();

        public event DataAvailable OnDataAvailable;

        private bool paused;
        public bool IsPaused {
            get => this.paused; 
            set => this.paused = value;
        }

        protected BaseConnectionThreaded(int sleepTime = 1) {
            this.sleepTime = sleepTime;
            this.IsPaused = true;
            this.thread = new Thread(this.ThreadMain);
            this.thread.Start();
        }

        private void ThreadMain() {
            while(this.notDisposing) {
                if (this.IsPaused) {
                    Thread.Sleep(this.sleepTime);
                    continue;
                }

                if (this.IsConnected) {
                    if (this.Stream != null) {
                        if (this.Stream.CanRead()) {
                            OnDataAvailable?.Invoke();
                        }
                    }
                }

                Thread.Sleep(this.sleepTime);
            }
        }
    }
}
