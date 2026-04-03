using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CryptoApiLiteSharp;

namespace Smev3Client.Crypt
{
    public sealed class GostAsymmetricAlgorithm : AsymmetricAlgorithm
    {
        private const byte SignBuffSize = 64;

        private readonly uint _keySpec;

        private CspSafeHandle? _cspHandle;
        private CertStoreSafeHandle? _storeHandle;
        private CertContextSafeHandle? _certHandle;

        private readonly Lazy<byte[]> _certRawData;

        static GostAsymmetricAlgorithm()
        {
            CryptoConfig.AddAlgorithm(typeof(GostAsymmetricAlgorithm), "Smev3Signature");
            CryptoConfig.AddAlgorithm(
                typeof(GostSignatureDescription),
                XmlDsigConsts.XmlDsigGost3410_2012_256Url
            );
            CryptoConfig.AddAlgorithm(
                typeof(GostR3411_2012_256HashAlgorithm),
                XmlDsigConsts.XmlDsigGost3411_2012_256Url
            );
        }

        private GostAsymmetricAlgorithm()
        {
            _certRawData = new Lazy<byte[]>(GetCertRawData, true);
        }

        ~GostAsymmetricAlgorithm()
        {
            Dispose(false);
        }

        public override string SignatureAlgorithm => XmlDsigConsts.XmlDsigGost3410_2012_256Url;

        public byte[] CertRawData => _certRawData.Value;

        private static readonly object Lock = new object();

        public unsafe GostAsymmetricAlgorithm(
            string pfxPath,
            string? pfxPassword,
            string thumbPrint
        )
            : this()
        {
            if (string.IsNullOrWhiteSpace(thumbPrint))
            {
                throw new ArgumentException("Отпечаток сертификата не может быть пустой строкой");
            }

            var pfxData = File.ReadAllBytes(pfxPath);

            try
            {
                fixed (byte* ptr = pfxData)
                {
                    var pfxDataBlob = new CRYPT_DATA_BLOB
                    {
                        cbData = pfxData.Length,
                        pbData = new IntPtr(ptr),
                    };

                    var passwordBytes = string.IsNullOrWhiteSpace(pfxPassword)
                        ? Array.Empty<byte>()
                        : Encoding.UTF32.GetBytes(pfxPassword);
                    fixed (byte* ptrPassword = passwordBytes)
                    {
                        lock (Lock)
                        {
                            _storeHandle =
                                CApiLiteNative.PFXImportCertStore(
                                    ref pfxDataBlob,
                                    new IntPtr(ptrPassword),
                                    CApiLiteConsts.CRYPT_MACHINE_KEYSET
                                        | CApiLiteConsts.PKCS12_IMPORT_SILENT
                                )
                                ?? throw new CApiLiteLastErrorException(
                                    nameof(CApiLiteNative.PFXImportCertStore)
                                );
                        }

                        if (_storeHandle.IsInvalid)
                        {
                            throw new CApiLiteLastErrorException(
                                nameof(CApiLiteNative.PFXImportCertStore)
                            );
                        }
                    }
                }

                var thumbPrintData = DecodeHexString(thumbPrint);

                fixed (byte* ptr = thumbPrintData)
                {
                    var thumbPrintDataBlob = new CRYPT_DATA_BLOB
                    {
                        cbData = thumbPrintData.Length,
                        pbData = new IntPtr(ptr),
                    };

                    _certHandle =
                        CApiLiteNative.CertFindCertificateInStore(
                            _storeHandle,
                            CApiLiteConsts.PKCS_7_OR_X509_ASN_ENCODING,
                            0,
                            CApiLiteConsts.CERT_FIND_SHA1_HASH,
                            new IntPtr(&thumbPrintDataBlob),
                            IntPtr.Zero
                        )
                        ?? throw new CApiLiteLastErrorException(
                            nameof(CApiLiteNative.CertFindCertificateInStore)
                        );
                    if (_certHandle.IsInvalid)
                    {
                        throw new CApiLiteLastErrorException(
                            nameof(CApiLiteNative.CertFindCertificateInStore)
                        );
                    }
                }

                var callerFreeProvider = false;
                if (
                    !CApiLiteNative.CryptAcquireCertificatePrivateKey(
                        _certHandle,
                        CApiLiteConsts.CRYPT_ACQUIRE_USE_PROV_INFO_FLAG,
                        IntPtr.Zero,
                        out _cspHandle,
                        ref _keySpec,
                        ref callerFreeProvider
                    )
                )
                {
                    throw new CApiLiteLastErrorException(
                        nameof(CApiLiteNative.CryptAcquireCertificatePrivateKey)
                    );
                }
            }
            catch
            {
                Dispose(true);
                throw;
            }
        }

        /// <summary>
        /// Подпись хэш
        /// </summary>
        public unsafe byte[] CreateHashSignature(byte[] hashData)
        {
            if (hashData is null || hashData.Length == 0)
            {
                throw new ArgumentException(
                    $"Параметр {nameof(hashData)} должен быть не пустым массивом."
                );
            }

            HashSafeHandle? hashHandle = null;
            try
            {
                if (
                    !CApiLiteNative.CryptCreateHash(
                        _cspHandle,
                        CApiLiteConsts.CALG_GR3411_2012_256,
                        IntPtr.Zero,
                        0,
                        out hashHandle
                    )
                )
                {
                    throw new CApiLiteLastErrorException(nameof(CApiLiteNative.CryptCreateHash));
                }

                fixed (void* ptrHashData = hashData)
                {
                    if (
                        !CApiLiteNative.CryptSetHashParam(
                            hashHandle,
                            CApiLiteConsts.HP_HASHVAL,
                            new IntPtr(ptrHashData),
                            0
                        )
                    )
                    {
                        throw new CApiLiteLastErrorException(
                            nameof(CApiLiteNative.CryptSetHashParam)
                        );
                    }

                    var signData = new byte[SignBuffSize];
                    var signDataLen = SignBuffSize;

                    fixed (byte* ptrSignData = signData)
                    {
                        if (
                            !CApiLiteNative.CryptSignHash(
                                hashHandle,
                                _keySpec,
                                IntPtr.Zero,
                                0,
                                new IntPtr(ptrSignData),
                                ref signDataLen
                            )
                        )
                        {
                            throw new CApiLiteLastErrorException(
                                nameof(CApiLiteNative.CryptSignHash)
                            );
                        }
                    }

                    Array.Reverse(signData);

                    return signData;
                }
            }
            finally
            {
                hashHandle?.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            _certHandle?.Close();
            _storeHandle?.Close();
            _cspHandle?.Close();

            _certHandle = null;
            _storeHandle = null;
            _cspHandle = null;

            base.Dispose(disposing);
        }

        #region private

        private unsafe byte[] GetCertRawData()
        {
            if (_certHandle is null || _certHandle.IsInvalid)
            {
                throw new InvalidOperationException("Объект не инициализирован");
            }

            var certContext = Marshal.PtrToStructure<CERT_CONTEXT>(
                _certHandle.DangerousGetHandle()
            );

            var certEncoded = new byte[certContext.cbCertEncoded];

            fixed (void* ptr = certEncoded)
            {
                Buffer.MemoryCopy(
                    certContext.pbCertEncoded.ToPointer(),
                    ptr,
                    certEncoded.Length,
                    certContext.cbCertEncoded
                );
            }

            return certEncoded;
        }

        private static byte[] DecodeHexString(string s)
        {
            var hexString = s.Replace(" ", "");
            var cbHex = (uint)hexString.Length / 2;
            var hex = new byte[cbHex];
            var i = 0;
            for (var index = 0; index < cbHex; index++)
            {
                hex[index] = (byte)((HexToByte(hexString[i]) << 4) | HexToByte(hexString[i + 1]));
                i += 2;
            }
            return hex;
        }

        private static byte HexToByte(char val)
        {
            if (val <= '9' && val >= '0')
                return (byte)(val - '0');

            if (val >= 'a' && val <= 'f')
                return (byte)(val - 'a' + 10);

            if (val >= 'A' && val <= 'F')
                return (byte)(val - 'A' + 10);

            return 0xFF;
        }

        #endregion
    }
}
