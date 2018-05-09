using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Retrosheet.Utilities.GameLogUtility
{
    public class StringCompressor
    {
        public string Compress(string text)
        {
            var textBuffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();

            using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                zipStream.Write(textBuffer, 0, textBuffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var zipBuffer = new byte[compressedData.Length + 4];

            Buffer.BlockCopy(compressedData, 0, zipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(textBuffer.Length), 0, zipBuffer, 0, 4);

            return Convert.ToBase64String(zipBuffer);
        }

        public string Decompress(string text)
        {
            var zipBuffer = Convert.FromBase64String(text);

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(zipBuffer, 4, zipBuffer.Length - 4);
                memoryStream.Position = 0;

                var dataLength = BitConverter.ToInt32(zipBuffer, 0);
                var buffer = new byte[dataLength];
                
                using (var zipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    zipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}