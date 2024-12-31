using System.Buffers.Binary;
using System.Text;

namespace HaruhiGekidouLib.Util;

public static class IO
{
    public static short ReadShort(byte[] data, int offset)
    {
        return BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(offset..(offset + 2)));
    }
    
    public static ushort ReadUShort(byte[] data, int offset)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset..(offset + 2)));
    }
    
    public static int ReadInt(byte[] data, int offset)
    {
        return BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset..(offset + 4)));
    }
    
    public static uint ReadUInt(byte[] data, int offset)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset..(offset + 4)));
    }

    public static string ReadShiftJisString(byte[] data, int offset)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding("Shift-JIS").GetString(data.Skip(offset).TakeWhile(b => b != 0x00).ToArray());
    }

    public static string ReadAsciiString(byte[] data, int offset)
    {
        return Encoding.ASCII.GetString(data.Skip(offset).TakeWhile(b => b != 0x00).ToArray());
    }
}