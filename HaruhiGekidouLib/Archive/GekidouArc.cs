using System.Text;
using HaruhiGekidouLib.Util;

namespace HaruhiGekidouLib.Archive;

public class GekidouArc
{
    public const uint MAGIC = 0x55AA382D;

    public List<GekidouArcEntry> Entries { get; set; } = [];

    public GekidouArc()
    {
    }
    
    public GekidouArc(byte[] data)
    {
        if (IO.ReadInt(data, 0x00) != MAGIC)
        {
            throw new InvalidDataException("Not a valid Gekidou archive!");
        }
        
        int fileTableOffset = IO.ReadInt(data, 0x04);

        int numFiles = IO.ReadInt(data, fileTableOffset + 0x08);
        int stringTableOffset = fileTableOffset + numFiles * 0x0C;

        for (int i = 0; i < numFiles; i++)
        {
            Entries.Add(new(data, fileTableOffset, i, stringTableOffset));
        }
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = [];
        List<byte> data = [];
        int fileTableLength = Entries.Count * 0x0C + Entries.Sum(e => e.Name.Length + 1);
        //fileTableLength += (fileTableLength % 2 == 0 ? 0 : 1);    
        int firstFileOffset = 0x20 + fileTableLength + (0x20 - fileTableLength % 0x20);
        
        bytes.AddRange(IO.GetUIntBytes(MAGIC));
        bytes.AddRange(IO.GetIntBytes(0x20)); 
        bytes.AddRange(IO.GetIntBytes(fileTableLength));
        bytes.AddRange(IO.GetIntBytes(firstFileOffset));    
        bytes.AddRange(new byte[0x10]); //16 blank bytes

        // Add first entry manually
        bytes.AddRange(IO.GetShortBytes(0x100));
        bytes.AddRange(new byte [6]); // name offset 0, depth 0
        bytes.AddRange(IO.GetIntBytes(Entries.Count));
        
        short nameOffset = 1;
        foreach (GekidouArcEntry entry in Entries.Skip(1))
        {
            bytes.AddRange(IO.GetShortBytes((short)(entry.IsDirectory ? 0x100 : 0)));   //1 if its a directory, 0 if not
            bytes.AddRange(IO.GetShortBytes(nameOffset));
            nameOffset += (short)(entry.Name.Length + 1);
            if (entry.IsDirectory)
            {
                bytes.AddRange(IO.GetIntBytes(entry.OffsetOrDepth));
                bytes.AddRange(IO.GetIntBytes(entry.LengthOrLastItemIdx));
            }
            else
            {
                bytes.AddRange(IO.GetIntBytes(entry.OffsetOrDepth));
                bytes.AddRange(IO.GetIntBytes(entry.Data.Length)); //length of entry
                data.AddRange(entry.Data);  //store the entry's data for later
                data.AddRange(new byte[0x20 - data.Count % 0x20 ]); //pads to the next 4 bytes?
                //List<byte> entryData = [];
                //entryData.AddRange(entry.Data);
                //entryData.AddRange(new byte[0x20 - data.Count % 0x20 ]);
                //data.InsertRange(entry.OffsetOrDepth,entryData);
            }
        }
        
        foreach (GekidouArcEntry entry in Entries)
        {
            bytes.AddRange(Encoding.ASCII.GetBytes(entry.Name));    //add the name of the file
            bytes.Add(0);
        }
        bytes.AddRange(new byte[0x20 - bytes.Count % 0x20]); //pads to the next 4 bytes?

        bytes.AddRange(data);
        
        bytes.RemoveRange(bytes.Count-0x20,0x20);   //remove 32 empty bytes - works for tutorial arcs
        
        return [.. bytes];
    }
}

public class GekidouArcEntry
{
    public string Name { get; set; }
    public bool IsDirectory { get; set; }
    public int OffsetOrDepth { get; set; }
    public int LengthOrLastItemIdx { get; set; }
    
    public byte[] Data { get; set; } = [];

    public GekidouArcEntry(string name, bool isDirectory, int depth = 0, int lastItemIdx = 0, byte[]? data = null)
    {
        Name = name;
        IsDirectory = isDirectory;
        OffsetOrDepth = depth;
        LengthOrLastItemIdx = lastItemIdx;
        Data = data ?? [];
    }
    
    public GekidouArcEntry(byte[] data, int fileTableOffset, int idx, int nameTableOffset)
    {
        int entryOffset = fileTableOffset + 0x0C * idx;
        IsDirectory = IO.ReadShort(data, entryOffset) == 256;
        int nameOffset = IO.ReadShort(data, entryOffset + 0x02);
        OffsetOrDepth = IO.ReadInt(data, entryOffset + 0x04);
        LengthOrLastItemIdx = IO.ReadInt(data, entryOffset + 0x08);
        Name = idx == 0 ? string.Empty : IO.ReadAsciiString(data, nameTableOffset + nameOffset);

        if (!IsDirectory)
        {
            Data = data[OffsetOrDepth..(OffsetOrDepth + LengthOrLastItemIdx)];
            
        }
    }
}