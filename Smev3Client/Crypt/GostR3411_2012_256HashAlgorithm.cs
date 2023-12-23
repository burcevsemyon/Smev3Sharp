using System;
using System.Security.Cryptography;

using CryptoApiLiteSharp;

namespace Smev3Client.Crypt
{
    public class GostR3411_2012_256HashAlgorithm : HashAlgorithm
    {
        private static readonly CspSafeHandle _cspHandle;
        
        private HashSafeHandle _hashHandle;

        private bool _disposed;

        static GostR3411_2012_256HashAlgorithm()
        {
            if (!CApiLiteNative.CryptAcquireContext(
               out _cspHandle, null, CApiLiteConsts.CP_GR3410_2012_PROV,
               CApiLiteConsts.PROV_GOST_2012_256, CApiLiteConsts.CRYPT_VERIFYCONTEXT))
            {
                throw new CApiLiteLastErrorException();
            }
        }

        public GostR3411_2012_256HashAlgorithm()
        {
            HashSizeValue = 256;            
        }

        ~GostR3411_2012_256HashAlgorithm()
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

        protected unsafe override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            ThrowIfDisposed();

            if (_hashHandle == null || _hashHandle.IsClosed || _hashHandle.IsInvalid)
            {
                if (!CApiLiteNative.CryptCreateHash(
                    _cspHandle, CApiLiteConsts.CALG_GR3411_2012_256, IntPtr.Zero,
                    0, out _hashHandle))
                {
                    throw new CApiLiteLastErrorException();
                }
            }

            if (array.Length == 0 || cbSize == 0)
            {
                return;
            }

            fixed(byte* pbData = &array[ibStart])
            {
                if (!CApiLiteNative.CryptHashData(_hashHandle, new IntPtr(pbData), cbSize, 0))
                {
                    throw new CApiLiteLastErrorException();
                }
            }
        }

        protected unsafe override byte[] HashFinal()
        {
            ThrowIfDisposed();

            int dataLength = 32;
            var data = new byte[dataLength];

            fixed(void* ptr = data)
            {                
                if (!CApiLiteNative.CryptGetHashParam(
                    _hashHandle, CApiLiteConsts.HP_HASHVAL, new IntPtr(ptr), ref dataLength, 0))
                {
                    throw new CApiLiteLastErrorException();
                }
            }

            if(dataLength != data.Length)
            {
                throw new Exception("Неверный размер хэша!");
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
            if (_hashHandle != null)
            {
                _ = CApiLiteNative.CryptDestroyHash(_hashHandle.DangerousGetHandle());

                _hashHandle.Close();
            }

            _hashHandle = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GostR3411_2012_256HashAlgorithm));
            }
        }
    }
}
