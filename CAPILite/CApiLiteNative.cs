using System;
using System.Text;
using System.Runtime.InteropServices;

namespace CAPILite
{
    public static class CApiLiteNative
    {
        [DllImport("capi20.so", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern 
        bool CryptAcquireContext(
            [Out] out CspSafeHandle phProv,
            [In] string pszContainer,
            [In] string pszProvider,
            [In] uint dwProvType,
            [In] uint dwFlags);

        [DllImport("capi20.so", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern 
        bool CryptReleaseContext(
            [In] IntPtr hProv,
            [In] uint dwFlags);

        [DllImport("capi20.so")]
        public static extern 
        int GetLastError();

        [DllImport("capi20.so", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern 
        int FormatMessage(
            [In] uint dwFlags,
            [In] IntPtr lpSource,
            [In] int dwMessageId,
            [In] int dwLanguageId, 
            [Out] StringBuilder lpBuffer,
            [In] int nSize,
            [In] IntPtr va_list_arguments);

        [DllImport("capi20.so", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern 
            CertStoreSafeHandle PFXImportCertStore(
            [In] ref CRYPT_DATA_BLOB pPfx,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szPassword,
            [In] uint dwFlags);

        [DllImport("capi20.so", SetLastError = true)]
        public static extern
            CertContextSafeHandle CertFindCertificateInStore(
            [In] CertStoreSafeHandle hCertStore,
            [In] uint dwCertEncodingType,
            [In] uint dwFindFlags,
            [In] uint dwFindType,
            [In] IntPtr pvFindPara,
            [In] IntPtr pPrevCertContext);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern
        bool CertCloseStore(
            [In] IntPtr hCertStore,
            [In] uint dwFlags);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern 
        bool CertFreeCertificateContext(
            [In] IntPtr pCertContext);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static
        bool CryptAcquireCertificatePrivateKey(
            [In] CertContextSafeHandle pCert,
            [In] uint dwFlags,
            [In] IntPtr pvReserved,
            [Out] out CspSafeHandle phCryptProv,
            [In, Out] ref uint pdwKeySpec,
            [In, Out] ref bool pfCallerFreeProv);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern
        bool CryptCreateHash(
            [In] CspSafeHandle hProv,
            [In] uint Algid,
            [In] IntPtr hKey,
            [In] int dwFlags,
            [Out] out HashSafeHandle phHash);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern
        bool CryptDestroyHash(
          [In] IntPtr hHash
        );


        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern
        bool CryptHashData(
            [In] HashSafeHandle hHash,
            [In] IntPtr pbData,
            [In] int dataLen,
            [In] int flags);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern 
        bool CryptGetHashParam(
            [In] HashSafeHandle hHash,
            [In] uint dwParam,
            [In, Out] IntPtr pbData,
            [In, Out] ref int pdwDataLen,
            [In] int dwFlags);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern
        bool CryptSetHashParam(
            [In] HashSafeHandle hHash,
            [In] uint dwParam,
            [In] IntPtr pbData,
            [In] int dwFlags);

        [DllImport("capi20.so", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern 
        bool CryptSignHash(
            [In] HashSafeHandle hHash,
            [In] uint keySpec,
            [In] IntPtr description,
            [In] uint flags,
            [Out] IntPtr signature,
            [In, Out] ref int signatureLen);
    }
}
