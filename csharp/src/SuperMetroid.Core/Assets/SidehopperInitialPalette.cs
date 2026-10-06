namespace SuperMetroid.Core.Assets;

/// <summary>
/// $A9:EBCC-EBEA and $F8C6-F8E4 share the initial Sidehopper palette. The drain
/// imports its first15 words; the live target imports16. White slots1/9 and
/// repeated slots14=11/15=12 and the body/detail/claw shades calculate. The standalone16-color target uses reviewed paints; the shifted15-color stock auxiliary has a separate calculated paint owner.
/// Each instance owns its supplied content; edits never couple separate catalogs.
/// </summary>
internal sealed class SidehopperInitialPalette
{
    /// <summary>$A9:EBD2/EBD4 and $F8CC/F8CE: cyan body/leg colors3180 and18C0.
    /// The exact shadow halves each RGB5 channel. This selected ratio is reviewed for the standalone target only; the shifted auxiliary disposition is separate.</summary>
    private const int BodyShadowDivisor = 2;
    private readonly ushort[] independentOrSupplied;
    private readonly bool calculated;
    private readonly bool stockTarget;
    internal int Count { get; }

    internal SidehopperInitialPalette(ushort[] colors)
    {
        Count = colors.Length;
        if (Count is not (15 or 16)) throw new ArgumentException("Sidehopper palette requires15 or16 colors.", nameof(colors));
        if (Count == 16 && Enumerable.Range(0, Count).All(color => colors[color] == SidehopperInitialPaintDefinitions.Color(color)))
        {
            stockTarget = true;
            independentOrSupplied = [];
            return;
        }
        calculated = colors[7] == DetailMidpoint(colors[6], colors[8]) && colors[11] == ClawMidpoint(colors[10], colors[12]) && colors[4] == BodyShadow(colors[3]) && colors[1] == 0x7fff && colors[9] == 0x7fff && colors[14] == colors[11] &&
            (Count == 15 || colors[15] == colors[12]);
        if (!calculated) { independentOrSupplied = colors; return; }
        independentOrSupplied = new ushort[9];
        independentOrSupplied[0] = colors[0];
        colors.AsSpan(2, 2).CopyTo(independentOrSupplied.AsSpan(1));
        colors.AsSpan(5, 2).CopyTo(independentOrSupplied.AsSpan(3));
        independentOrSupplied[5] = colors[8];
        independentOrSupplied[6] = colors[10];
        colors.AsSpan(12, 2).CopyTo(independentOrSupplied.AsSpan(7));
    }

    internal ushort Resolve(int color)
    {
        if ((uint)color >= Count) throw new ArgumentOutOfRangeException(nameof(color));
        if (stockTarget) return SidehopperInitialPaintDefinitions.Color(color);
        if (!calculated) return independentOrSupplied[color];
        return color switch
        {
            1 or 9 => (31 << 10) | (31 << 5) | 31,
            11 => ClawMidpoint(Resolve(10), Resolve(12)),
            14 => Resolve(11),
            15 => Resolve(12),
            0 => independentOrSupplied[0],
            4 => BodyShadow(Resolve(3)),
            7 => DetailMidpoint(Resolve(6), Resolve(8)),
            < 4 => independentOrSupplied[color - 1],
            < 7 => independentOrSupplied[color - 2],
            < 9 => independentOrSupplied[color - 3],
            < 11 => independentOrSupplied[color - 4],
            _ => independentOrSupplied[color - 5],
        };
    }
    /// <summary>$A9:EBDA/$F8D4, inner-detail ink7 is the channel-floor midpoint
    /// of installed inner-detail paints at slots6/8 (5EC0/35A0).</summary>
    internal static ushort DetailMidpoint(ushort light, ushort dark)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((light >> shift & 31) + (dark >> shift & 31)) / 2) << shift;
        return (ushort)result;
    }
    internal static ushort BodyShadow(ushort light)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= ((light >> shift & 31) / BodyShadowDivisor) << shift;
        return (ushort)result;
    }
    /// <summary>$A9:EBE2/$F8DC: yellow claw shade0237 is the ceiling-channel midpoint of039C/00D1.
    /// Independently supplied light/dark endpoints remain editable.</summary>
    internal static ushort ClawMidpoint(ushort light, ushort dark)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((light >> shift & 31) + (dark >> shift & 31) + 1) / 2) << shift;
        return (ushort)result;
    }
}
