using System.IO;
using System.Threading.Tasks;

namespace REghZyIOWrapperV2.Streams {
    public interface IDataInput {
        Stream Stream { get; set; }

        /// <summary>
        /// Closes the stream
        /// </summary>
        void Close();

        /// <summary>
        /// Reads the given number of bytes from the stream
        /// </summary>
        /// <param name="buffer">The buffer to read into</param>
        /// <param name="offset">The offset in the buffer</param>
        /// <param name="count">The number of bytes to read</param>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Reads 1 byte and return true if its value is 1, otherwise false
        /// </summary>
        bool ReadBool();

        /// <summary>
        /// Reads 1 <see cref="byte"/>
        /// </summary>
        byte ReadByte();

        /// <summary>
        /// Reads 2 <see cref="byte"/>s and joins them into a <see cref="ushort"/> value
        /// </summary>
        ushort ReadShort();

        /// <summary>
        /// Reads 4 <see cref="byte"/>s and joins them into a <see cref="uint"/> value
        /// </summary>
        uint ReadInt();

        /// <summary>
        /// Reads 8 <see cref="byte"/>s and joins them into a <see cref="ulong"/> value
        /// </summary>
        ulong ReadLong();

        /// <summary>
        /// Reads 4 <see cref="byte"/>s, joins them into a <see cref="uint"/> value, and uses unsafe code to cast it to a <see cref="float"/>
        /// </summary>
        float ReadFloat();

        /// <summary>
        /// Reads 8 <see cref="byte"/>s, joins them into a <see cref="ulong"/> value, and uses unsafe code to cast it to a <see cref="double"/>
        /// </summary>
        double ReadDouble();

        /// <summary>
        /// Reads 2 <see cref="byte"/>s, joins them into a <see cref="ushort"/> value, and casts it to a <see cref="char"/>
        /// </summary>
        char ReadChar();

        /// <summary>
        /// Reads the given number of characters, and joins them into a string
        /// </summary>
        /// <param name="len">The number of characters to read</param>
        /// <returns></returns>
        string ReadString(int len);
    }
}