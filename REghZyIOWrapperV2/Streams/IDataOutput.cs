using System;
using System.IO;

namespace REghZyIOWrapperV2.Streams {
    /// <summary>
    /// An interface for writing primitive data to a stream
    /// </summary>
    public interface IDataOutput {
        Stream Stream { get; set; }

        /// <summary>
        /// Flushes the data to the stream
        /// </summary>
        void Flush();

        /// <summary>
        /// Closes the stream
        /// </summary>
        void Close();

        /// <summary>
        /// Writes the given number of bytes, starting at the given offset, from the given buffer
        /// </summary>
        /// <param name="buffer">The buffer to write data from</param>
        /// <param name="offset">The index to start reading from the buffer</param>
        /// <param name="count">The number of bytes to write</param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Writes a boolean value (1 byte)
        /// </summary>
        /// <param name="val"></param>
        void WriteBoolean(bool val);

        /// <summary>
        /// Writes an enum value as a byte
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        void WriteEnum8<TEnum>(TEnum value) where TEnum : unmanaged, Enum;

        /// <summary>
        /// Writes an enum value as a short (2 bytes)
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        void WriteEnum16<TEnum>(TEnum value) where TEnum : unmanaged, Enum;

        /// <summary>
        /// Writes an enum value as an integer (4 bytes)
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        void WriteEnum32<TEnum>(TEnum value) where TEnum : unmanaged, Enum;

        /// <summary>
        /// Writes an enum value as a long value (8 bytes)
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        void WriteEnum64<TEnum>(TEnum value) where TEnum : unmanaged, Enum;

        /// <summary>
        /// Writes a single byte
        /// </summary>
        /// <param name="value"></param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes a short (2 bytes)
        /// </summary>
        /// <param name="value"></param>
        void WriteShort(ushort value);

        /// <summary>
        /// Writes an integer (4 bytes)
        /// </summary>
        /// <param name="value"></param>
        void WriteInt(uint value);

        /// <summary>
        /// Writes a long (8 bytes)
        /// </summary>
        /// <param name="value"></param>
        void WriteLong(ulong value);

        /// <summary>
        /// Writes a floating point number (4 bytes)
        /// </summary>
        /// <param name="value"></param>
        void WriteFloat(float value);

        /// <summary>
        /// Writes a double percision floating point number (8 bytes)
        /// </summary>
        /// <param name="value"></param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a char (2 bytes, exact same as <see cref="WriteShort(ushort)"/>)
        /// </summary>
        /// <param name="value"></param>
        void WriteChar(char value);

        /// <summary>
        /// Writes all of the chars in the given string
        /// </summary>
        /// <param name="value"></param>
        void WriteString(string value);

        /// <summary>
        /// Writes all of the chars in the given string
        /// </summary>
        /// <param name="chars"></param>
        void WriteChars(char[] chars);
    }
}