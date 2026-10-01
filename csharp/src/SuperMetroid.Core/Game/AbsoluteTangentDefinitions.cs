namespace SuperMetroid.Core.Game;

/// <summary>Exact native 8.8 absolute ray gradients, including the horizontal infinity substitute.</summary>
public static class AbsoluteTangentDefinitions
{
    /// <summary>$91:C9D4, AbsoluteTangentTable: first quarter of native absolute
    /// 8.8 gradients, approximately |tan(t*pi/128)|*256, with an infinity substitute.</summary>
    /// <remarks>
    /// #1165 independently reviewed: retain the 65-word quarter table and exact
    /// index reflection after comparing all 129 original NTSC J/U v1.0 words and
    /// pinned bank_91.asm. Domain 0..128 includes both zero endpoints, exact diagonal
    /// 256 and finite infinity substitute 15360. No extrapolation or modulo clamping.
    /// Historical reduced-pi model floor(256*tan(n*3.14159/128)), n=min(i,128-i),
    /// needs explicit diagonal/infinity cases. True pi differs at 63/65 (10428 versus
    /// native 10427); reduced pi alone makes the diagonal 255. These are plausible
    /// historical precision conventions, not evidence of the original generator.
    /// A deterministic quotient/error-bound evaluator is substantially more complex
    /// and costly than 65 direct words in per-frame ray/window construction. Reflection
    /// already removes the duplicate half. Keep this exact compact representation.
    /// Eye windows, Mother Brain and X-ray direction rendering share this one mapping;
    /// SamusXrayRomData.Window.AbsoluteTangentTable is an address alias, not more data.
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
