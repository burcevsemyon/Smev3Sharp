using System;
using System.Security.Cryptography;

namespace Smev3Client.Utils
{
    public static class TimeGuid
    {
        private static readonly DateTimeOffset StartDate = new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);

        public static Guid NewGuid()
        {
            Span<byte> guidBytes = stackalloc byte[16];

            var elapsed = DateTimeOffset.UtcNow - StartDate;
            var ticks = elapsed.Ticks;

            guidBytes[0] = (byte)(ticks & 0xFF);
            guidBytes[1] = (byte)((ticks >> 8) & 0xFF);
            guidBytes[2] = (byte)((ticks >> 16) & 0xFF);
            guidBytes[3] = (byte)((ticks >> 24) & 0xFF);
            guidBytes[4] = (byte)((ticks >> 32) & 0xFF);
            guidBytes[5] = (byte)((ticks >> 40) & 0xFF);
            guidBytes[6] = (byte)((ticks >> 48) & 0xFF);
            guidBytes[7] = (byte)((ticks >> 56) & 0xFF);

            // Set version bits (version 1 UUID) - using bitwise OR for better performance
            guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x10);

            // Set variant bits (RFC 4122 variant)
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

            // Generate random clock sequence and node ID
            RandomNumberGenerator.Fill(guidBytes[10..]);

            return new Guid(guidBytes);
        }
    }
}
