using System.Runtime.InteropServices;
using System.Security;

namespace MinimalGallery.API;

[SuppressUnmanagedCodeSecurity]
internal static class WinApiCalls
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    public static extern int StrCmpLogicalW(string psz1, string psz2);
}
