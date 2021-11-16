using System.IO;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting {
    public static class PacketUtils {
        public static int GetBytesWL(this string str) {
            return 2 + (str.Length * 2);
        }

        public static int GetBytesNL(this string str) {
            return str.Length * 2;
        }

        /**
         * Writes 2 bytes (a short, being the length of the string), and 2 bytes foreach each character of the given char array
         * @param value The array of chars to write (maximum length allowed is 65535)
         * @param output The data output to write to
         * @throws IOException Thrown if there was an IOException while writing a character
         */
        public static void WriteCharsWL(char[] value, IDataOutput output) {
            output.WriteShort((ushort) value.Length);
            output.WriteChars(value);
        }

        /// <summary>
        /// Writes 2 bytes for each character of the given character array
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <param name="output">The data output to write to</param>
        public static void WriteCharsNL(char[] value, IDataOutput output) {
            output.WriteChars(value);
        }

        /// <summary>
        /// Writes 2 bytes (a short, being the length of the string), and 2 bytes for each character of the given character array
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <param name="output">The data output to write to</param>
        public static void WriteStringWL(string value, IDataOutput output) {
            output.WriteShort((ushort) value.Length);
            output.WriteString(value);
        }

        /// <summary>
        /// Writes 2 bytes for each character of the given character array
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <param name="output">The data output to write to</param>
        public static void WriteStringNL(string value, IDataOutput output) {
            output.WriteString(value);
        }

        /// <summary>
        /// Reads 2 bytes (being the length of a string) as a short value, and reads that many characters
        /// </summary>
        /// <param name="input">The data input</param>
        /// <returns>A string</returns>
        public static string ReadStringWL(IDataInput input) {
            ushort length = input.ReadShort();

            try {
                return input.ReadString(length);
            }
            catch (IOException e) {
                throw new IOException("Failed to read string (of length " + length + ")", e);
            }
        }

        public static string ReadStringNL(IDataInput input, int length) {
            return input.ReadString(length);
        }

        public static byte[] ReadBytesWL(IDataInput input) {
            ushort length = input.ReadShort();
            byte[] buffer = new byte[length];
            input.ReadFully(buffer, 0, length);
            return buffer;
        }
    }
}