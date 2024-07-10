using System;
using System.Security.Cryptography;

namespace Smev3Client.Utils
{
    public static class Rfc4122
    {
        private static readonly DateTimeOffset _startDate = new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);

        private static byte[] GetCalendarStartElapsedTicksBytes()
        {
            return BitConverter.GetBytes((DateTimeOffset.UtcNow - _startDate).Ticks);
        }

        public static unsafe Guid GenerateUUIDv1()
        {
            Span<byte> uuidBytes = stackalloc byte[16];

            var ticksBytes = GetCalendarStartElapsedTicksBytes();
            for (var i = 0; i < ticksBytes.Length; i++)
            {
                uuidBytes[i] = ticksBytes[i];
            }

            RandomNumberGenerator.Fill(uuidBytes[8..]);

            // version V1
            uuidBytes[7] &= 0x0f;
            uuidBytes[7] |= 1 << 4;

            // layout
            uuidBytes[8] &= 0x3f;
            uuidBytes[8] |= 0x80;

            return new Guid(uuidBytes);
        }
    }
}
