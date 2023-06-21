﻿using System;

namespace Smev3Client.Utils
{
    public static class Rfc4122
    {
        private static readonly object _syncObject = new object();

        private static readonly DateTimeOffset GregorianCalendarStart = new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);

        private static readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public static Guid GenerateUUIDv1()
        {
            var uuidBytes = new byte[16];
            
            lock (_syncObject)
            {
                Array.Copy(
                    BitConverter.GetBytes(
                        (DateTimeOffset.UtcNow - GregorianCalendarStart).Ticks), uuidBytes, 8);

                _random.NextBytes(new Span<byte>(uuidBytes, 8, 8));                
            }

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
