using System;
using System.IO;
using System.Threading.Tasks;

namespace REghZyIOWrapperV2.Streams {
    /// <summary>
    /// An interface for reading primitive data from a stream
    /// </summary>
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
        /// Reads the exact number of bytes (specified by the given buffer's size) into the buffer (starting at 0)
        /// <para>
        /// Invoking this is the exact same as invoking <see cref="ReadFully(byte[], int, int)"/>, 
        /// where the offset is 0 and the length is the given buffer's length
        /// </para>
        /// </summary>
        /// <param name="buffer">The buffer to put the bytes into</param>
        void ReadFully(byte[] buffer);

        /// <summary>
        /// Reads the exact given number of bytes into the given buffer (starting at the given offset)
        /// <para>
        /// The size of the buffer WILL NOT be checked, so it will throw an out of bounds exception if you mess up
        /// </para>
        /// </summary>
        /// <param name="buffer">The buffer to put bytes into</param>
        /// <param name="offset">The index specifying where to start writing into the buffer (inclusive)</param>
        /// <param name="length">The number of bytes to read (e.g 4 for an integer, 2 for a short, etc)</param>
        void ReadFully(byte[] buffer, int offset, int length);

        /// <summary>
        /// Reads 1 byte and return true if its value is 1, otherwise false
        /// </summary>
        bool ReadBool();

        /// <summary>
        /// Reads 1 byte and converts it to an enum. This requires that the enum type's size is equal to the 
        /// size of a byte, otherwise you will lose the extra data, resulting in undefined behaviour
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>The enum itself</returns>
        T ReadEnum8<T>() where T : unmanaged, Enum;

        /// <summary>
        /// Reads 2 bytes and converts it to an enum. This requires that the enum type's size is smaller than or equal to the 
        /// size of a short/ushort, otherwise you will lose the extra data, resulting in undefined behaviour
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>The enum itself</returns>
        T ReadEnum16<T>() where T : unmanaged, Enum;

        /// <summary>
        /// Reads 4 bytes and converts it to an enum. This requires that the enum type's size is smaller than or equal to the 
        /// size of a int/uint, otherwise you will lose the extra data, resulting in undefined behaviour
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>The enum itself</returns>
        T ReadEnum32<T>() where T : unmanaged, Enum;

        /// <summary>
        /// Reads 8 bytes and converts it to an enum. This requires that the enum type's size is smaller than or equal to the 
        /// size of a long/ulong, otherwise you will lose the extra data, resulting in undefined behaviour
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>The enum itself</returns>
        T ReadEnum64<T>() where T : unmanaged, Enum;

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
        /// <para>
        /// Invoking this is the exact same as invoking <see cref="ReadChars(int)"/>, only passing the char array into the string's constructor
        /// </para>
        /// </summary>
        /// <param name="len">The number of characters to read</param>
        /// <returns></returns>
        string ReadString(int len);

        /// <summary>
        /// Reads the given number of characters into a character array
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        char[] ReadChars(int len);
    }
}