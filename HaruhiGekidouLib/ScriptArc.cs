using HaruhiGekidouLib.Util;

namespace HaruhiGekidouLib;

public class ScriptArc
{
    public const uint MAGIC = 0x55AA382D;

    public List<ScriptArcEntry> Entries { get; set; } = [];

    public ScriptArc(byte[] data)
    {
        int fileTableOffset = IO.ReadInt(data, 0x04);

        int numFiles = IO.ReadInt(data, fileTableOffset + 0x08);
        int stringTableOffset = fileTableOffset + numFiles * 0x0C;

        for (int i = 0; i < numFiles; i++)
        {
            Entries.Add(new(data, fileTableOffset, i, stringTableOffset));
        }
    }
}

public class ScriptArcEntry
{
    public string Name { get; set; }
    public bool IsDirectory { get; set; }
    public short NameOffset { get; set; }
    public int OffsetOrDepth { get; set; }
    public int LengthOrLastItemIdx { get; set; }
    
    public byte[] Data { get; set; }

    public ScriptArcEntry(byte[] data, int fileTableOffset, int idx, int nameTableOffset)
    {
        int entryOffset = fileTableOffset + 0x0C * idx;
        IsDirectory = IO.ReadShort(data, entryOffset) == 256;
        NameOffset = IO.ReadShort(data, entryOffset + 0x02);
        OffsetOrDepth = IO.ReadInt(data, entryOffset + 0x04);
        LengthOrLastItemIdx = IO.ReadInt(data, entryOffset + 0x08);
        Name = idx == 0 ? string.Empty : IO.ReadAsciiString(data, nameTableOffset + NameOffset);

        if (!IsDirectory)
        {
            Data = data[OffsetOrDepth..(OffsetOrDepth + LengthOrLastItemIdx)];
        }
    }
}