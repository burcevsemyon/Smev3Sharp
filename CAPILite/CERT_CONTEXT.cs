using System;
using System.Runtime.InteropServices;

namespace CAPILite
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CERT_CONTEXT
    {
        public uint dwCertEncodingType;        
        public IntPtr pbCertEncoded;
        public uint cbCertEncoded;
        public IntPtr pCertInfo;
        public IntPtr hCertStore;
    }
}
