using System;
using System.IO;
using System.Text;

namespace REghZyIOWrapperV2.Streams {
    /// <summary>
    /// A class for reading primitive objects from a stream
    /// <para>
    /// Most method have repeated code for speed reasons...
    /// </para>
    /// </summary>
    public class DataInputStream : IDataInput {
        private Stream stream;

        private readonly byte[] buffer = new byte[8];

        public Stream Stream {
            get => this.stream;
            set => this.stream = value;
        }

        public DataInputStream(Stream stream) {
            this.stream = stream;
        }

        public void Close() {
            this.stream.Close();
        }

        public int Read(byte[] buffer, int offset, int count) {
            return this.stream.Read(buffer, offset, count);
        }

        public bool ReadBool() {
            if (this.stream.Read(this.buffer, 0, 1) != 1) {
                throw new EndOfStreamException("Failed to read 1 byte for a boolean");
            }

            return this.buffer[0] == 1;
        }

        public byte ReadByte() {
            int read = this.stream.Read(this.buffer, 0, 1);
            if (read != 1) {
                throw new EndOfStreamException("Failed to read 1 byte for a byte");
            }

            return this.buffer[0];
        }

        public ushort ReadShort() {
            byte[] b = this.buffer;
            if (this.stream.Read(b, 0, 2) != 2) {
                throw new EndOfStreamException("Failed to read 2 bytes for a ushort");
            }

            return (ushort) ((b[0] << 8) + (b[1] << 0));
        }

        public uint ReadInt() {
            byte[] b = this.buffer;
            if (this.stream.Read(b, 0, 4) != 4) {
                throw new EndOfStreamException("Failed to read 4 bytes for a uint");
            }

            return (((uint) b[0]) << 24) +
                   (((uint) b[1]) << 16) +
                   (((uint) b[2]) << 8) +
                   (((uint) b[3]) << 0);
        }

        public ulong ReadLong() {
            byte[] b = this.buffer;
            if (this.stream.Read(b, 0, 8) != 8) {
                throw new EndOfStreamException("Failed to read 8 bytes for a ulong");
            }

            return (((ulong) b[0]) << 56) +
                   (((ulong) b[1]) << 48) +
                   (((ulong) b[2]) << 40) +
                   (((ulong) b[3]) << 32) +
                   (((ulong) b[4]) << 24) +
                   (((ulong) b[5]) << 16) +
                   (((ulong) b[6]) << 8) +
                   (((ulong) b[7]) << 0);
        }

        public float ReadFloat() {
            byte[] b = this.buffer;
            if (this.stream.Read(b, 0, 4) != 4) {
                throw new EndOfStreamException("Failed to read 4 bytes for a float");
            }

            unsafe {
                uint p0 = ((uint) b[0] << 24) +
                          ((uint) b[1] << 16) +
                          ((uint) b[2] << 8) +
                          ((uint) b[3] << 0);
                return *(float*) &p0;
            }
        }

        public double ReadDouble() {
            byte[] b = this.buffer;
            if (this.stream.Read(b, 0, 8) != 8) {
                throw new EndOfStreamException("Failed to read 8 bytes for a double");
            }

            unsafe {
                ulong p0 = ((ulong) b[0] << 56) +
                           ((ulong) b[1] << 48) +
                           ((ulong) b[2] << 40) +
                           ((ulong) b[3] << 32) +
                           ((ulong) b[4] << 24) +
                           ((ulong) b[5] << 16) +
                           ((ulong) b[6] << 8) +
                           ((ulong) b[7] << 0);
                return *(double*) &p0;
            }
        }

        public char ReadChar() {
            byte[] b = this.buffer;
            if (this.stream.Read(b, 0, 2) != 2) {
                throw new EndOfStreamException("Failed to read 2 bytes for a char");
            }

            return (char) (ushort) ((b[0] << 0) + (b[1] << 8));
        }

        public string ReadString(int len) {
            if (len == 0) {
                return string.Empty;
            }
            else {
                byte[] b = this.buffer;
                Stream s = this.stream;
                if (len == 1) {
                    if (s.Read(b, 0, 2) != 2) {
                        throw new EndOfStreamException("Failed to read 2 bytes for a char (in string len 1)");
                    }

                    return new string(new char[1] {(char) (ushort) ((b[0] << 8) + (b[1] << 0))});
                }
                else if (len == 2) {
                    if (s.Read(b, 0, 4) != 4) {
                        throw new EndOfStreamException("Failed to read 4 bytes for 2 chars (in string len 2)");
                    }

                    return new string(new char[2] {
                        (char) (ushort) ((b[0] << 8) + (b[1] << 0)),
                        (char) (ushort) ((b[2] << 8) + (b[3] << 0)),
                    });
                }
                else if (len == 3) {
                    if (s.Read(b, 0, 6) != 6) {
                        throw new EndOfStreamException("Failed to read 6 bytes for 3 chars (in string len 3)");
                    }

                    return new string(new char[3] {
                        (char) (ushort) ((b[0] << 8) + (b[1] << 0)),
                        (char) (ushort) ((b[2] << 8) + (b[3] << 0)),
                        (char) (ushort) ((b[4] << 8) + (b[5] << 0)),
                    });
                }
                else {
                    StringBuilder builder = new StringBuilder(len);
                    while (true) {
                        if (len > 3) {
                            if (s.Read(b, 0, 8) != 8) {
                                throw new EndOfStreamException("Failed to read 8 bytes for 4 chars (in unknown string len)");
                            }

                            builder.Append((char) (ushort) ((b[0] << 8) + (b[1] << 0)));
                            builder.Append((char) (ushort) ((b[2] << 8) + (b[3] << 0)));
                            builder.Append((char) (ushort) ((b[4] << 8) + (b[5] << 0)));
                            builder.Append((char) (ushort) ((b[6] << 8) + (b[7] << 0)));
                            len -= 4;
                        }
                        else {
                            if (len == 3) {
                                if (s.Read(b, 0, 6) != 6) {
                                    throw new EndOfStreamException("Failed to read 6 bytes for 3 chars (in unknown string len, last 3 chars)");
                                }

                                builder.Append((char) (ushort) ((b[0] << 8) + (b[1] << 0)));
                                builder.Append((char) (ushort) ((b[2] << 8) + (b[3] << 0)));
                                builder.Append((char) (ushort) ((b[4] << 8) + (b[5] << 0)));
                            }
                            else if (len == 2) {
                                if (s.Read(b, 0, 4) != 4) {
                                    throw new EndOfStreamException("Failed to read 4 bytes for 2 chars (in unknown string len, last 2 chars)");
                                }

                                builder.Append((char) (ushort) ((b[0] << 8) + (b[1] << 0)));
                                builder.Append((char) (ushort) ((b[2] << 8) + (b[3] << 0)));
                            }
                            else if (len == 1) {
                                if (s.Read(b, 0, 2) != 2) {
                                    throw new EndOfStreamException("Failed to read 2 bytes for 1 char (in unknown string len, last 1 char)");
                                }

                                builder.Append((char) (ushort) ((b[0] << 8) + (b[1] << 0)));
                            }

                            return builder.ToString();
                        }
                    }
                }
            }
        }
    }
}