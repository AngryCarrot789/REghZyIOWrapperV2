using System;
using System.IO;

namespace REghZyIOWrapperV2.Streams {
    /// <summary>
    /// A stream wrapper that supports blocking reading
    /// </summary>
    public class BlockingStream : Stream {
        private readonly Stream stream;

        public Stream BaseStream { get => this.stream; }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position {
            get => stream.Position; 
            set => stream.Position = value;
        }

        private readonly byte[] read1 = new byte[1];

        public BlockingStream(Stream stream) {
            if (stream == null) {
                throw new NullReferenceException("Stream cannot be null");
            }

            this.stream = stream;
        }

        public override void Flush() {
            this.stream.Flush();
        }

        /// <summary>
        /// Reads from the stream, blocking until it reads the given count
        /// </summary>
        /// <param name="buffer">The buffer to read into</param>
        /// <param name="offset">The offset to start reading into the buffer</param>
        /// <param name="count">The exact number of bytes to read</param>
        /// <returns>Exactly the specified number of bytes</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            int a = 0;
            Stream s = this.stream;
            while (count > 0) {
                int r = s.Read(buffer, offset + a, count);
                a += (r < 0 ? 0 : r);
                count -= r;
            }

            return a;
        }

        /// <summary>
        /// Blocks until a single byte can be read
        /// </summary>
        /// <returns>A single byte</returns>
        public override int ReadByte() {
            Stream s = this.stream;
            byte[] b = this.read1;
            while (s.Read(b, 0, 1) != 1) { }
            return b[0];
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing) {
            this.stream.Dispose();
        }
    }
}
