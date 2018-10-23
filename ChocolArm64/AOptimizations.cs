using System.Runtime.Intrinsics.X86;

public static class AOptimizations
{
    internal static bool FastFp = true;

    private static bool _useAllSseIfAvailable = true;

    private static bool _useSseIfAvailable   = true;
    private static bool _useSse2IfAvailable  = true;
    private static bool _useSse41IfAvailable = true;
    private static bool _useSse42IfAvailable = true;

    internal static bool UseSse   = (_useAllSseIfAvailable && _useSseIfAvailable)   && Sse.IsSupported;
    internal static bool UseSse2  = (_useAllSseIfAvailable && _useSse2IfAvailable)  && Sse2.IsSupported;
    internal static bool UseSse41 = (_useAllSseIfAvailable && _useSse41IfAvailable) && Sse41.IsSupported;
    internal static bool UseSse42 = (_useAllSseIfAvailable && _useSse42IfAvailable) && Sse42.IsSupported;
}
