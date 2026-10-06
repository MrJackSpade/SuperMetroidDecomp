namespace SuperMetroid.Core.Assets;

/// <summary>
/// $A9:F8A6-F8C4 and $EC8C-ECAA share corpse paint. Slot5 is white; slots8..15
/// interpolate RGB5 endpoints to nearest over seven intervals. The auxiliary
/// import includes only slots0..14; its stock paint has a separate calculated owner. This helper preserves independently supplied15-color content. The standalone16-word target uses reviewed paints and material composition.
/// </summary>
internal sealed class SidehopperCorpsePalette
{
    private readonly ushort[] independentOrSupplied;
    private readonly bool calculated;
    private readonly bool stockTarget;
    internal int Count { get; }
    /// <summary>$A9:ECAA/$F8C4, the ramp dark endpoint also used at final-drain $EC74.
    /// The fifteen-color import exposes this dependency only when its samples uniquely determine the endpoint.</summary>
    internal ushort? ReconstructedDarkEndpoint => stockTarget ? SidehopperCorpsePaintDefinitions.Color(15) : calculated ? independentOrSupplied[8] : null;

    internal SidehopperCorpsePalette(ushort[] colors)
    {
        Count = colors.Length;
        if (Count is not (15 or 16)) throw new ArgumentException("Corpse palette requires15 or16 colors.", nameof(colors));
        if (Count == 16 && Enumerable.Range(0, Count).All(color => colors[color] == SidehopperCorpsePaintDefinitions.Color(color)))
        {
            stockTarget = true;
            independentOrSupplied = [];
            return;
        }
        independentOrSupplied = colors;
        if (colors[5] != 0x7fff) return;
        ushort endpoint = Count == 16 ? colors[15] : (ushort)0;
        if (Count == 15)
        {
            // Intersect the exact integer intervals admitted by each rounded
            // sample. Ambiguous or incompatible independent content stays raw.
            for (int shift = 0; shift < 15; shift += 5)
            {
                int start = colors[8] >> shift & 31;
                int lower = 0, upper = 31;
                for (int phase = 1; phase < Count - 8; phase++)
                {
                    int sample = colors[8 + phase] >> shift & 31;
                    int numerator = sample * 7 - start * (7 - phase) - 3;
                    lower = Math.Max(lower, (int)Math.Ceiling((double)numerator / phase));
                    upper = Math.Min(upper, (int)Math.Floor((double)(numerator + 6) / phase));
                }
                if (lower != upper) return;
                endpoint |= (ushort)(lower << shift);
            }
        }
        for (int color = 8; color < Count; color++)
            if (Interpolate(colors[8], endpoint, color - 8) != colors[color]) return;
        independentOrSupplied = new ushort[9];
        colors.AsSpan(0, 5).CopyTo(independentOrSupplied);
        colors.AsSpan(6, 3).CopyTo(independentOrSupplied.AsSpan(5));
        independentOrSupplied[8] = endpoint;
        calculated = true;
    }

    internal ushort Resolve(int color)
    {
        if ((uint)color >= Count) throw new ArgumentOutOfRangeException(nameof(color));
        if (stockTarget) return SidehopperCorpsePaintDefinitions.Color(color);
        if (!calculated) return independentOrSupplied[color];
        if (color == 5) return (31 << 10) | (31 << 5) | 31;
        if (color < 8) return independentOrSupplied[color < 5 ? color : color - 1];
        return Interpolate(independentOrSupplied[7], independentOrSupplied[8], color - 8);
    }

    internal static ushort Interpolate(ushort start, ushort end, int phase)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((start >> shift & 31) * (7 - phase) + (end >> shift & 31) * phase + 3) / 7) << shift;
        return (ushort)result;
    }
}
