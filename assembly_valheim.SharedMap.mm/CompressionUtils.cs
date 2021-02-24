using System.Collections;
using System.IO;
using System.IO.Compression;

namespace ValheimSharedMap
{
    internal static class CompressionUtils
    {
        public static byte[] Compress(bool[] input)
        {
            using (var memoryStream = new MemoryStream())
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            {
                var buffer = new byte[input.Length / 8 + (input.Length % 8 == 0 ? 0 : 1)];
                new BitArray(input).CopyTo(buffer, 0);

                deflateStream.Write(buffer, 0, buffer.Length);
                deflateStream.Close();

                return memoryStream.ToArray();
            }
        }

        public static bool[] Decompress(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                deflateStream.CopyTo(outputStream);
                deflateStream.Close();

                var buffer = outputStream.ToArray();
                var output = new bool[buffer.Length * 8];
                new BitArray(buffer).CopyTo(output, 0);

                return output;
            }
        }
    }
}