using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// $A9:F8A6-F8C4 and $EC8C-ECAA share corpse paint. Slot5 is white; slots8..15
/// interpolate RGB5 endpoints to nearest over seven intervals. The auxiliary
/// import includes only slots0..14; its stock paint has a separate calculated owner. This helper preserves independently supplied15-color content. The standalone16-word target uses reviewed paints and material composition.
/// </summary>
internal sealed class SidehopperCorpsePalette
{
    private readonly Bgr555[] independentOrSupplied;
    private readonly bool calculated;
    private readonly bool stockTarget;
    internal int Count { get; }

    internal SidehopperCorpsePalette(Bgr555[] colors)
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
        if (colors[5] != Bgr555.White) return;
        Bgr555 endpoint = Count == 16 ? colors[15] : Bgr555.Black;
        if (Count == 15)
        {
            // Intersect the exact integer intervals admitted by each rounded
            // sample. Ambiguous or incompatible independent content stays raw.
            Span<int> endpointChannels = stackalloc int[3];
            foreach (ColorChannel channel in Enum.GetValues<ColorChannel>())
            {
                int start = colors[8][channel];
                int lower = 0, upper = 31;
                for (int phase = 1; phase < Count - 8; phase++)
                {
                    int sample = colors[8 + phase][channel];
                    int numerator = sample * 7 - start * (7 - phase) - 3;
                    lower = Math.Max(lower, (int)Math.Ceiling((double)numerator / phase));
                    upper = Math.Min(upper, (int)Math.Floor((double)(numerator + 6) / phase));
                }
                if (lower != upper) return;
                endpointChannels[(int)channel] = lower;
            }
            endpoint = new(endpointChannels[0], endpointChannels[1], endpointChannels[2]);
        }
        for (int color = 8; color < Count; color++)
            if (Interpolate(colors[8], endpoint, color - 8) != colors[color]) return;
        independentOrSupplied = new Bgr555[9];
        colors.AsSpan(0, 5).CopyTo(independentOrSupplied);
        colors.AsSpan(6, 3).CopyTo(independentOrSupplied.AsSpan(5));
        independentOrSupplied[8] = endpoint;
        calculated = true;
    }

    internal Bgr555 Resolve(int color)
    {
        if ((uint)color >= Count) throw new ArgumentOutOfRangeException(nameof(color));
        if (stockTarget) return SidehopperCorpsePaintDefinitions.Color(color);
        if (!calculated) return independentOrSupplied[color];
        if (color == 5) return new Bgr555(31, 31, 31);
        if (color < 8) return independentOrSupplied[color < 5 ? color : color - 1];
        return Interpolate(independentOrSupplied[7], independentOrSupplied[8], color - 8);
    }

    internal static Bgr555 Interpolate(Bgr555 start, Bgr555 end, int phase)
    {
        return start.Zip(end, (_, a, b) => ((a * (7 - phase) + b * phase + 3) / 7));
    }
}
