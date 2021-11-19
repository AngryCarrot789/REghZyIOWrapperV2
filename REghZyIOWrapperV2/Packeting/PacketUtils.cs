using System;
using System.IO;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting {
    public static class PacketUtils {
        public static int GetBytesWL(this string value) {
            if (value == null || value.Length == 0) {
                return 2;
            }
            else {
                return 2 + (value.Length * 2);
            }
        }

        public static int GetBytesNL(this string value) {
            if (value == null || value.Length == 0) {
                return 0;
            }
            else {
                return value.Length * 2;
            }
        }

        /**
         * Writes 2 bytes (a short, being the length of the string), and 2 bytes foreach each character of the given char array
         * @param value The array of chars to write (maximum length allowed is 65535)
         * @param output The data output to write to
         * @throws IOException Thrown if there was an IOException while writing a character
         */
        public static void WriteCharsWL(char[] value, IDataOutput output) {
            if (value == null || value.Length == 0) {
                output.WriteShort(0);
            }
            else {
                output.WriteShort((ushort) value.Length);
                output.WriteChars(value);
            }
        }

        /// <summary>
        /// Writes 2 bytes for each character of the given character array
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <param name="output">The data output to write to</param>
        public static void WriteCharsNL(char[] value, IDataOutput output) {
            if (value == null) {
                throw new Exception("Value cannot be null");
            }

            output.WriteChars(value);
        }

        /// <summary>
        /// Writes 2 bytes (a short, being the length of the string), and 2 bytes for each character of the given character array
        /// <para>
        /// If the given string is null, it will simply write 2 bytes of value '0' (resulting in an empty string being received on the other side)
        /// </para>
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <param name="output">The data output to write to</param>
        public static void WriteStringWL(string value, IDataOutput output) {
            if (value == null || value.Length == 0) {
                output.WriteShort(0);
            }
            else {
                output.WriteShort((ushort) value.Length);
                output.WriteString(value);
            }
        }

        /// <summary>
        /// Writes 2 bytes for each character of the given character array
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <param name="output">The data output to write to</param>
        public static void WriteStringNL(string value, IDataOutput output) {
            if (value == null) {
                throw new Exception("Value cannot be null");
            }

            output.WriteString(value);
        }

        /// <summary>
        /// Reads 2 bytes (being the length of a string) as a short value, and reads that many characters
        /// </summary>
        /// <param name="input">The data input</param>
        /// <returns>A string</returns>
        public static string ReadStringWL(IDataInput input) {
            ushort length = input.ReadShort();
            if (length == 0) {
                return null;
            }

            try {
                return input.ReadStringUTF16(length);
            }
            catch (IOException e) {
                throw new IOException("Failed to read string (of length " + length + ")", e);
            }
        }

        public static string ReadStringNL(IDataInput input, int length) {
            return input.ReadStringUTF16(length);
        }

        public static byte[] ReadBytesWL(IDataInput input) {
            ushort length = input.ReadShort();
            byte[] buffer = new byte[length];
            if (length > 0) {
                input.ReadFully(buffer, 0, length);
            }

            return buffer;
        }
    }
}