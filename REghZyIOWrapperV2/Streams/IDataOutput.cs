using System.IO;

namespace REghZyIOWrapperV2.Streams {
    public interface IDataOutput {
        Stream Stream { get; set; }

        void Flush();
        void Close();

        void Write(byte[] buffer, int offset, int count);

        void WriteBoolean(bool val);

        void WriteByte(byte value);

        void WriteShort(ushort value);

        void WriteInt(uint value);

        void WriteLong(ulong value);

        void WriteFloat(float value);

        void WriteDouble(double value);

        void WriteChar(char value);

        void WriteString(string value);
    }
}