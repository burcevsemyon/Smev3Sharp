using System;
using System.Security.Cryptography;
using CryptoApiLiteSharp;

namespace Smev3Client.Crypt
{
    public class GostHashAlgorithm : HashAlgorithm
    {
        private static readonly CspSafeHandle CspHandle;

        private HashSafeHandle? _hashHandle;

        private bool _disposed;

        static GostHashAlgorithm()
        {
            if (
                !CApiLiteNative.CryptAcquireContext(
                    out CspHandle,
                    null,
                    CApiLiteConsts.CP_GR3410_2012_PROV,
                    CApiLiteConsts.PROV_GOST_2012_256,
                    CApiLiteConsts.CRYPT_VERIFYCONTEXT
                )
            )
            {
                throw new CApiLiteLastErrorException(nameof(CApiLiteNative.CryptAcquireContext));
            }
        }

        public GostHashAlgorithm()
        {
            HashSizeValue = 256;
        }

        ~GostHashAlgorithm()
        {
            Dispose(false);
        }

        public override int InputBlockSize => 64;

        public override int OutputBlockSize => 64;

        public override void Initialize()
        {
            ThrowIfDisposed();

            ResetHash();
        }

        protected override unsafe void HashCore(byte[] array, int ibStart, int cbSize)
        {
            ThrowIfDisposed();

            if (_hashHandle is null || _hashHandle.IsClosed || _hashHandle.IsInvalid)
            {
                if (
                    !CApiLiteNative.CryptCreateHash(
                        CspHandle,
                        CApiLiteConsts.CALG_GR3411_2012_256,
                        IntPtr.Zero,
                        0,
                        out _hashHandle
                    )
                )
                {
                    throw new CApiLiteLastErrorException(nameof(CApiLiteNative.CryptCreateHash));
                }
            }

            if (array.Length == 0 || cbSize == 0)
            {
                return;
            }

            fixed (byte* pbData = &array[ibStart])
            {
                if (!CApiLiteNative.CryptHashData(_hashHandle, new IntPtr(pbData), cbSize, 0))
                {
                    throw new CApiLiteLastErrorException(nameof(CApiLiteNative.CryptHashData));
                }
            }
        }

        protected override unsafe byte[] HashFinal()
        {
            ThrowIfDisposed();

            var dataLength = 32;
            var data = new byte[dataLength];

            fixed (void* ptr = data)
            {
                if (
                    !CApiLiteNative.CryptGetHashParam(
                        _hashHandle,
                        CApiLiteConsts.HP_HASHVAL,
                        new IntPtr(ptr),
                        ref dataLength,
                        0
                    )
                )
                {
                    throw new CApiLiteLastErrorException(nameof(CApiLiteNative.CryptGetHashParam));
                }
            }

            return data;
        }

        protected override void Dispose(bool disposing)
        {
            ResetHash();

            base.Dispose(disposing);

            _disposed = true;
        }

        private void ResetHash()
        {
            _hashHandle?.Close();
            _hashHandle = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GostHashAlgorithm));
            }
        }
    }
}
