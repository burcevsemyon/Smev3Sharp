using System;

using Microsoft.Win32.SafeHandles;

namespace CAPILite
{
    public class CertStoreSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected CertStoreSafeHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            var result = CApiLiteNative.CertCloseStore(handle, 0);

            handle = IntPtr.Zero;

            return result;
        }
    }
}
