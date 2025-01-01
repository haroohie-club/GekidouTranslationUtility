using AuroraLib.Compression.Algorithms;

namespace HaruhiGekidouLib.Util;

public static class Compression
{
    public static byte[] Decompress(byte[] compressedBytes)
    {
        LZ11 lz11 = new();
        return lz11.Decompress(compressedBytes);
    }

    public static byte[] Compress(byte[] decompressedBytes)
    {
        LZ11 lz11 = new();
        return lz11.Compress(decompressedBytes).ToArray();
    }
}