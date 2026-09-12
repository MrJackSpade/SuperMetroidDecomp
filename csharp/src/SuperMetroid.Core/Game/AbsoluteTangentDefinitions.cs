namespace SuperMetroid.Core.Game;

/// <summary>Exact native 8.8 absolute ray gradients, including the horizontal infinity substitute.</summary>
public static class AbsoluteTangentDefinitions
{
    /// <summary>$91:C9D4, AbsoluteTangentTable: first quarter of |tan(t*pi/128)|*256, truncated to native words.</summary>
    private static ReadOnlySpan<ushort> Quarter =>
    [
        0,6,12,18,25,31,37,44,50,57,64,70,77,84,91,98,
        106,113,121,128,136,145,153,162,171,180,189,199,210,220,232,243,
        256,268,282,296,311,328,345,363,383,404,427,451,478,508,541,577,
        618,663,715,774,843,925,1022,1140,1286,1475,1725,2075,2599,3470,5210,10427,15360,
    ];

    /// <summary>Indices 0..128 cover the complete native table; index 128 is an explicit final zero.</summary>
    public static ushort Sample(int index)
    {
        if ((uint)index > 128) throw new ArgumentOutOfRangeException(nameof(index));
        return Quarter[index <= 64 ? index : 128 - index];
    }
}
