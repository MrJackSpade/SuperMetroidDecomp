namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// $A9:F8A6-F8C4 and $EC8C-ECAA share corpse paint. Slot5 is white; slots8..15
/// interpolate RGB5 endpoints to nearest over seven intervals. The auxiliary
/// import includes only slots0..14; its stock paint has a separate calculated owner. This helper preserves independently supplied15-color content. The standalone16-word target uses reviewed paints and material composition.
/// </summary>
internal sealed class SidehopperCorpsePalette
{
    /// <summary>Stores either the complete supplied colors or independent colors plus a fade endpoint.</summary>
    internal readonly RgbColor[] independentOrSupplied;
    /// <summary>Marks palettes whose white slot and seven-step gradient can be reproduced exactly.</summary>
    private readonly bool calculated;
    /// <summary>Uses reviewed stock paint definitions for a matching sixteen-color target.</summary>
    private readonly bool stockTarget;
    /// <summary>Number of colors in the imported fifteen-color prefix or full target palette.</summary>
    [IntegerQuantity("number of colors in the palette row")]
    internal int Count { get; }

    /// <summary>Preserves supplied paint unless its gradient has one exact reproducible endpoint.</summary>
    internal SidehopperCorpsePalette(RgbColor[] colors)
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
        if (colors[5] != RgbColor.White) return;
        RgbColor endpoint = Count == 16 ? colors[15] : RgbColor.Black;
        if (Count == 15)
        {
            // Ambiguous or incompatible independent content stays raw.
            if (EndpointChannel(colors, ColorChannel.Red) is not { } red ||
                EndpointChannel(colors, ColorChannel.Green) is not { } green ||
                EndpointChannel(colors, ColorChannel.Blue) is not { } blue)
                return;
            endpoint = new(red, green, blue);
        }
        for (int color = 8; color < Count; color++)
            if (Interpolate(colors[8], endpoint, color - 8) != colors[color]) return;
        independentOrSupplied = new RgbColor[9];
        colors.AsSpan(0, 5).CopyTo(independentOrSupplied);
        colors.AsSpan(6, 3).CopyTo(independentOrSupplied.AsSpan(5));
        independentOrSupplied[8] = endpoint;
        calculated = true;
    }

    /// <summary>
    /// The one endpoint channel value every rounded sample of the fifteen-colour gradient admits, found by intersecting
    /// the exact integer intervals each sample allows, or null when the samples admit none or several.
    /// </summary>
    private static int? EndpointChannel(RgbColor[] colors, ColorChannel channel)
    {
        int start = colors[8][channel];
        int lower = 0, upper = 31;
        for (int phase = 1; phase < colors.Length - 8; phase++)
        {
            int sample = colors[8 + phase][channel];
            int numerator = sample * 7 - start * (7 - phase) - 3;
            lower = Math.Max(lower, (int)Math.Ceiling((double)numerator / phase));
            upper = Math.Min(upper, (int)Math.Floor((double)(numerator + 6) / phase));
        }
        return lower == upper ? lower : null;
    }

    /// <summary>Resolves a stock, independently supplied, or reconstructed corpse palette color.</summary>
    internal RgbColor Resolve(int color)
    {
        if ((uint)color >= Count) throw new ArgumentOutOfRangeException(nameof(color));
        if (stockTarget) return SidehopperCorpsePaintDefinitions.Color(color);
        if (!calculated) return independentOrSupplied[color];
        if (color == 5) return new RgbColor(31, 31, 31);
        if (color < 8) return independentOrSupplied[color < 5 ? color : color - 1];
        return Interpolate(independentOrSupplied[7], independentOrSupplied[8], color - 8);
    }

    /// <summary>Interpolates RGB5 channels with the cartridge's seven-interval nearest rounding.</summary>
    internal static RgbColor Interpolate(RgbColor start, RgbColor end, int phase)
    {
        return start.Zip(end, (_, a, b) => ((a * (7 - phase) + b * phase + 3) / 7));
    }
}
