﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using CAPILite;

namespace Smev3Client.Crypt
{
    public class GostAsymmetricAlgorithm : AsymmetricAlgorithm
    {
        private CspSafeHandle _cspHandle;
        private CertStoreSafeHandle _storeHandle;
        private CertContextSafeHandle _certHandle;
        private readonly uint _keySpec;

        private readonly Lazy<byte[]> _certRawData;

        static GostAsymmetricAlgorithm()
        {
            CryptoConfig.AddAlgorithm(typeof(GostAsymmetricAlgorithm), "Smev3Signature");
            CryptoConfig.AddAlgorithm(typeof(GostSignatureDescription), "urn:ietf:params:xml:ns:cpxmlsec:algorithms:gostr34102012-gostr34112012-256");
            CryptoConfig.AddAlgorithm(typeof(GostR3411_2012_256HashAlgorithm), "urn:ietf:params:xml:ns:cpxmlsec:algorithms:gostr34112012-256");
        }

        protected GostAsymmetricAlgorithm() 
        {
            _certRawData = new Lazy<byte[]>(() => GetCertRawData());
        }

        public unsafe GostAsymmetricAlgorithm(string pfxPath, string pfxPassword, string thumbPrint)
         :this()
        {
            var pfxData = File.ReadAllBytes(pfxPath);

            try
            {
                fixed (byte* ptr = pfxData)
                {
                    var pfxDataBlob = new CRYPT_DATA_BLOB
                    {
                        cbData = pfxData.Length,
                        pbData = new IntPtr(ptr)
                    };

                    _storeHandle = CApiLiteNative.PFXImportCertStore(ref pfxDataBlob, pfxPassword,
                        CApiLiteConsts.CRYPT_MACHINE_KEYSET | CApiLiteConsts.PKCS12_IMPORT_SILENT);
                    if (_storeHandle.IsInvalid)
                    {
                        throw new CApiLiteLastErrorException();
                    }
                }
                
                var thumbPrintData = DecodeHexString(thumbPrint);

                fixed (byte* ptr = thumbPrintData)
                {
                    var thumbPrintDataBlob = new CRYPT_DATA_BLOB
                    {
                        cbData = thumbPrintData.Length,
                        pbData = new IntPtr(ptr)
                    };

                    _certHandle = CApiLiteNative.CertFindCertificateInStore(
                        _storeHandle, CApiLiteConsts.PKCS_7_OR_X509_ASN_ENCODING, 0,
                        CApiLiteConsts.CERT_FIND_SHA1_HASH, new IntPtr(&thumbPrintDataBlob), IntPtr.Zero);
                    if (_certHandle.IsInvalid)
                    {
                        throw new CApiLiteLastErrorException();
                    }
                }
                
                bool callerFreeProvider = false;
                if (!CApiLiteNative.CryptAcquireCertificatePrivateKey(
                    _certHandle, CApiLiteConsts.CRYPT_ACQUIRE_USE_PROV_INFO_FLAG,
                    IntPtr.Zero, out _cspHandle, ref _keySpec, ref callerFreeProvider))
                {
                    throw new CApiLiteLastErrorException();
                }
            }
            catch
            {
                Dispose(true);
                throw;
            }
        }

        public unsafe byte[] CreateHashSignature(byte[] hashData)
        {
            if(hashData == null || hashData.Length == 0)
            {
                throw new ArgumentException("Отсутствуют данные для создания подписи.");
            }

            HashSafeHandle hashHandle = null;
            try
            {
                if (!CApiLiteNative.CryptCreateHash(
                    _cspHandle, CApiLiteConsts.CALG_GR3411_2012_256, IntPtr.Zero,
                    0, out hashHandle))
                {
                    throw new CApiLiteLastErrorException();
                }

                fixed (void* ptrHashData = hashData)
                {
                    if (!CApiLiteNative.CryptSetHashParam(hashHandle, CApiLiteConsts.HP_HASHVAL, new IntPtr(ptrHashData), 0))
                    {
                        throw new CApiLiteLastErrorException();
                    }

                    var signData = new byte[64];
                    int signDataLen = signData.Length;

                    fixed (byte* ptrSignData = signData)
                    {
                        if (!CApiLiteNative.CryptSignHash(hashHandle, _keySpec, IntPtr.Zero, 0, new IntPtr(ptrSignData), ref signDataLen))
                        {
                            throw new CApiLiteLastErrorException();
                        }
                    }

                    Array.Reverse(signData);

                    return signData;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                hashHandle?.Close();
            }
        }

        public byte[] CertRawData => _certRawData.Value;

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

        public override string SignatureAlgorithm => "urn:ietf:params:xml:ns:cpxmlsec:algorithms:gostr34102012-gostr34112012-256";

        private unsafe byte[] GetCertRawData()
        {
            if(_certHandle == null || _certHandle.IsInvalid)
            {
                throw new Exception("Объект не инициалирован.");
            }            

            var certContext = Marshal.PtrToStructure<CERT_CONTEXT>(
                                                    _certHandle.DangerousGetHandle());

            var certEncoded = new byte[certContext.cbCertEncoded];

            fixed(void* ptr = certEncoded)
            {
                Buffer.MemoryCopy(certContext.pbCertEncoded.ToPointer(), ptr,
                    certEncoded.Length, certContext.cbCertEncoded);
            }

            return certEncoded;
        }

        private static byte[] DecodeHexString(string s)
        {
            string hexString = s.Replace(" ", "");
            uint cbHex = (uint)hexString.Length / 2;
            byte[] hex = new byte[cbHex];
            int i = 0;
            for (int index = 0; index < cbHex; index++)
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
                return (byte)((val - 'a') + 10);

            if (val >= 'A' && val <= 'F')
                return (byte)((val - 'A') + 10);

            return 0xFF;
        }
    }
}
