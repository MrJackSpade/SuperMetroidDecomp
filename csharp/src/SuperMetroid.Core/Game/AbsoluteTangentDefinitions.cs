namespace SuperMetroid.Core.Game;

/// <summary>Exact native 8.8 absolute ray gradients, including the horizontal infinity substitute.</summary>
public static class AbsoluteTangentDefinitions
{
    /// <summary>$91:C9D4, AbsoluteTangentTable: first quarter of native absolute
    /// 8.8 gradients, approximately |tan(t*pi/128)|*256, with an infinity substitute.</summary>
    /// <remarks>
    /// Issue #625 exact algorithm: validate i in 0..128, then n=min(i,128-i).
    /// Return 256 at n=32 (exact diagonal), 15360 at n=64 (finite infinity), and
    /// otherwise floor(256*tan(n*3.14159/128)). This reproduces ALL 129 words,
    /// including zero at both endpoints. The short decimal pi is intentional:
    /// true pi gives 10428 at indices 63 and 65, but the cartridge stores 10427.
    /// The diagonal case is also necessary: the reduced-pi quotient gives 255.
    /// csharp/tools/LookupTableResearch verifies every result against the NTSC
    /// J/U v1.0 ROM, pinned bank_91.asm, and Sample. Its deterministic evaluator
    /// uses 24 decimal sine terms for sin(x)/cos(x), with cos(x) evaluated as
    /// sin(truePi/2-x), and proves the quotient's entire error interval has one
    /// integer floor. Do not use 3.14159/2-x as the cosine complement: that would
    /// cancel the phase error being reproduced. This is an exact reproduction
    /// recipe, not a claim about the original generator. Runtime migration and
    /// performance measurement remain for a later pass; the table is unchanged.
    /// </remarks>
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
