using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Separates the Ceres door's supplied colours into the material channels their paint calculates from and the colours
/// that depart from that calculation, the form the game resolves its door colours from.
/// </summary>
internal static class CeresDoorPaintRecognition
{
    /// <summary>Recognizes the normal, escape and animation paints of the installed door visuals.</summary>
    /// <param name="normal">Fifteen normal setup colours.</param>
    /// <param name="escape">Fifteen escape setup colours.</param>
    /// <param name="animation">Eight steps of six animation colours.</param>
    internal static CeresDoorVisuals Visuals(RgbColor[] normal, RgbColor[] escape, RgbColor[][] animation)
    {
        CeresDoorNormalPaint normalPaint = Normal(normal);
        return new(normalPaint, Escape(escape, normalPaint), Animation(animation, normalPaint));
    }

    /// <summary>Validates six supplied target colors, captures their shared channels, and preserves independently edited colors as overrides.</summary>
    internal static CeresDoorWarmTargetPaint WarmTargets(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 6);
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorWarmTargetPaint(new(colors[0].Blue, colors[1].Red, colors[1].Green, colors[2].Red,
            colors[3].Red, colors[3].Green, colors[3].Blue), edits);
        for (int index = 0; index < colors.Length; index++)
            if (paint.ColorAt(index) != colors[index]) edits.Add(new PaintColour(0, index), colors[index]);
        return paint;
    }

    /// <summary>Captures the selected surface parameters and preserves every independently edited supplied color.</summary>
    internal static CeresDoorEscapeSurfacePaint Surface(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 8);
        var levels = new int[4];
        for (int index = 0; index < levels.Length; index++) levels[index] = colors[index].Red;
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorEscapeSurfacePaint(levels, colors[4].Red, colors[7].Red, colors[5].Blue, edits);
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
                    seeds.Add(new CeresDoorPaintChannel(slot, channel), colors[slot - 1][channel]);
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
        var paint = new CeresDoorEscapePaint(warm, Surface(colors[..8]), colors[14].Blue, edits);
        for (int index = 0; index < colors.Length; index++)
            if (paint.ColorAt(index) != colors[index]) edits.Add(new PaintColour(0, index), colors[index]);
        return paint;
    }

    /// <summary>Captures independent paint channels and per-cell edits from the eight six-color steps of the glow cycle.</summary>
    internal static CeresDoorAnimationPaint Animation(RgbColor[][] steps, CeresDoorNormalPaint normal)
    {
        Ensure.NotNull(steps); Ensure.NotNull(normal);
        if (steps.Length != 8 || steps.Any(step => step is null || step.Length != 6))
            throw new ArgumentException("Beacon paint requires eight six-color steps.", nameof(steps));
        var seedEdits = new Dictionary<PaintColour, RgbColor>();
        var edits = new Dictionary<PaintColour, RgbColor>();
        var paint = new CeresDoorAnimationPaint(normal, steps[1][0].Blue, steps[3][4].Red, seedEdits, edits);
        for (int color = 0; color < 6; color++)
            if (steps[1][color] != paint.SharedSeed(color)) seedEdits.Add(new PaintColour(0, color), steps[1][color]);
        for (int step = 0; step < 8; step++)
            for (int color = 0; color < 6; color++)
                if (steps[step][color] != paint.ColorAt(step, color)) edits.Add(new PaintColour(step, color), steps[step][color]);
        return paint;
    }
}
