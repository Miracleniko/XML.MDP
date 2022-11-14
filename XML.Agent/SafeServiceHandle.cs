using System.Runtime.InteropServices;

namespace XML.Agent;

internal class SafeServiceHandle : SafeHandle
{
    public override Boolean IsInvalid
    {
        get
        {
            if (!(DangerousGetHandle() == IntPtr.Zero))
            {
                return DangerousGetHandle() == new IntPtr(-1);
            }
            return true;
        }
    }

    internal SafeServiceHandle(IntPtr handle)
    : base(IntPtr.Zero, true) => SetHandle(handle);

    protected override Boolean ReleaseHandle() => Advapi32.CloseServiceHandle(handle);
}