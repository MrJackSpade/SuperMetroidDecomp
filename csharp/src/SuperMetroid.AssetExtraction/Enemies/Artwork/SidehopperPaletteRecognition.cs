using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Recognizes a supplied Sidehopper palette as the reviewed stock paint, as independent paints whose dependent shades
/// calculate, or as independently supplied colors.
/// </summary>
internal static class SidehopperPaletteRecognition
{
    /// <summary>Selects stock, calculated, or independent paint ownership from the supplied initial palette values.</summary>
    internal static SidehopperInitialPalette Initial(RgbColor[] colors)
    {
        int count = colors.Length;
        if (count is not (15 or 16)) throw new ArgumentException("Sidehopper palette requires15 or16 colors.", nameof(colors));
        if (count == 16 && Enumerable.Range(0, count).All(color => colors[color] == SidehopperInitialPaint.Color(color)))
            return new(count, SidehopperPaletteOwnership.Stock, []);
        bool calculated = colors[7] == SidehopperInitialPalette.DetailMidpoint(colors[6], colors[8]) &&
            colors[11] == SidehopperInitialPalette.ClawMidpoint(colors[10], colors[12]) &&
            colors[4] == SidehopperInitialPalette.BodyShadow(colors[3]) &&
            colors[1] == RgbColor.White && colors[9] == RgbColor.White && colors[14] == colors[11] &&
            (count == 15 || colors[15] == colors[12]);
        if (!calculated)
            return new(count, SidehopperPaletteOwnership.Supplied, colors);
        var independent = new RgbColor[9];
        independent[0] = colors[0];
        colors.AsSpan(2, 2).CopyTo(independent.AsSpan(1));
        colors.AsSpan(5, 2).CopyTo(independent.AsSpan(3));
        independent[5] = colors[8];
        independent[6] = colors[10];
        colors.AsSpan(12, 2).CopyTo(independent.AsSpan(7));
        return new(count, SidehopperPaletteOwnership.Calculated, independent);
    }

    /// <summary>Preserves supplied corpse paint unless its gradient has one exact reproducible endpoint.</summary>
    internal static SidehopperCorpsePalette Corpse(RgbColor[] colors)
    {
        int count = colors.Length;
        if (count is not (15 or 16)) throw new ArgumentException("Corpse palette requires15 or16 colors.", nameof(colors));
        if (count == 16 && Enumerable.Range(0, count).All(color => colors[color] == SidehopperCorpsePaint.Color(color)))
            return new(count, SidehopperPaletteOwnership.Stock, []);
        var supplied = new SidehopperCorpsePalette(count, SidehopperPaletteOwnership.Supplied, colors);
        if (colors[5] != RgbColor.White) return supplied;
        RgbColor endpoint = count == 16 ? colors[15] : RgbColor.Black;
        if (count == 15)
        {
            // Ambiguous or incompatible independent content stays raw.
            if (EndpointChannel(colors, ColorChannel.Red) is not { } red ||
                EndpointChannel(colors, ColorChannel.Green) is not { } green ||
                EndpointChannel(colors, ColorChannel.Blue) is not { } blue)
                return supplied;
            endpoint = new(red, green, blue);
        }
        for (int color = 8; color < count; color++)
            if (SidehopperCorpsePalette.Interpolate(colors[8], endpoint, color - 8) != colors[color]) return supplied;
        var independent = new RgbColor[9];
        colors.AsSpan(0, 5).CopyTo(independent);
        colors.AsSpan(6, 3).CopyTo(independent.AsSpan(5));
        independent[8] = endpoint;
        return new(count, SidehopperPaletteOwnership.Calculated, independent);
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
}
