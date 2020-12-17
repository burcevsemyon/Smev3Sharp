using Microsoft.Win32.SafeHandles;

namespace CAPILite
{
    public class CertContextSafeHandle: SafeHandleZeroOrMinusOneIsInvalid
    {
        protected CertContextSafeHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (IsInvalid || IsClosed)
            {
                return true;
            }

            return CApiLiteNative.CertFreeCertificateContext(handle);
        }
    }
}
