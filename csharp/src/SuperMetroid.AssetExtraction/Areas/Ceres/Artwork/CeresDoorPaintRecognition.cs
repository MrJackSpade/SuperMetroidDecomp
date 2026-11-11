using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Separates Ceres-door-style colours into the material channels their paint calculates from and the colours that depart
/// from that calculation. The Ceres door keeps its installed colours whole; only the boss colour readers still use this.
/// </summary>
internal static class CeresDoorPaintRecognition
{
    /// <summary>Validates six supplied target colors, captures their shared channels, and preserves independently edited colors as overrides.</summary>
    internal static CeresDoorWarmTargetPaint WarmTargets(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 6);
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorWarmTargetPaint(new(colors[0].FadeLevels.Blue, colors[1].FadeLevels.Red, colors[1].FadeLevels.Green, colors[2].FadeLevels.Red,
            colors[3].FadeLevels.Red, colors[3].FadeLevels.Green, colors[3].FadeLevels.Blue), edits);
        for (int index = 0; index < colors.Length; index++)
            if (paint.ColorAt(index) != colors[index]) edits.Add(new PaintColour(0, index), colors[index]);
        return paint;
    }

    /// <summary>Captures the selected surface parameters and preserves every independently edited supplied color.</summary>
    internal static CeresDoorEscapeSurfacePaint Surface(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 8);
        var levels = new int[4];
        for (int index = 0; index < levels.Length; index++) levels[index] = colors[index].FadeLevels.Red;
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorEscapeSurfacePaint(levels, colors[4].FadeLevels.Red, colors[7].FadeLevels.Red, colors[5].FadeLevels.Blue, edits);
        for (int index = 0; index < colors.Length; index++)
            if (paint.ColorAt(index) != colors[index]) edits.Add(new PaintColour(0, index), colors[index]);
        return paint;
    }

    /// <summary>Separates chosen material paint channels from calculated and independently edited colors.</summary>
    internal static CeresDoorNormalPaint Normal(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 15);
        var seeds = new Dictionary<CeresDoorPaintChannel, int>();
        for (int slot = 1; slot <= colors.Length; slot++)
            foreach (ColorChannel channel in Enum.GetValues<ColorChannel>())
                if (StoresPaint(slot, channel))
                    seeds.Add(new CeresDoorPaintChannel(slot, channel), colors[slot - 1].FadeLevels[channel]);
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorNormalPaint(WarmTargets(colors.Slice(8, 6)), seeds, edits);
        for (int slot = 1; slot <= colors.Length; slot++)
            if (paint.ColorAt(slot - 1) != colors[slot - 1]) edits.Add(new PaintColour(0, slot), colors[slot - 1]);
        return paint;
    }

    /// <summary>Identifies channels whose values are authored seeds rather than derived hues.</summary>
    private static bool StoresPaint(int slot, ColorChannel channel) => channel switch
    {
        ColorChannel.Red => false,
        ColorChannel.Green => slot is 1 or 2 or 3 or 5 or 6 or 7,
        ColorChannel.Blue => slot is 3 or 4 or 5 or 6 or 7 or 8 or 15,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>Uses the normal door's warm targets when compiling the escape paint.</summary>
    internal static CeresDoorEscapePaint Escape(ReadOnlySpan<RgbColor> colors, CeresDoorNormalPaint normal) =>
        Escape(colors, Ensure.NotNull(normal).WarmTargets);

    /// <summary>Derives warm targets from the supplied escape palette when no normal palette is provided.</summary>
    internal static CeresDoorEscapePaint Escape(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 15);
        return Escape(colors, WarmTargets(colors.Slice(8, 6)));
    }

    /// <summary>Captures the derived material paint and retains every differing supplied ink as an edit.</summary>
    private static CeresDoorEscapePaint Escape(ReadOnlySpan<RgbColor> colors, CeresDoorWarmTargetPaint warm)
    {
        Ensure.LengthEqual(colors, 15);
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorEscapePaint(warm, Surface(colors[..8]), colors[14].FadeLevels.Blue, edits);
        for (int index = 0; index < colors.Length; index++)
            if (paint.ColorAt(index) != colors[index]) edits.Add(new PaintColour(0, index), colors[index]);
        return paint;
    }
}
