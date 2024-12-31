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

    public static byte[] GetShortBytes(short value)
    {
        var bytes = new byte[2];
        BinaryPrimitives.WriteInt16BigEndian(bytes, value);
        return bytes;
    }

    public static byte[] GetUShortBytes(ushort value)
    {
        var bytes = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        return bytes;
    }
    
    public static byte[] GetIntBytes(int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        return bytes;
    }
    
    public static byte[] GetUIntBytes(uint value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        return bytes;
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