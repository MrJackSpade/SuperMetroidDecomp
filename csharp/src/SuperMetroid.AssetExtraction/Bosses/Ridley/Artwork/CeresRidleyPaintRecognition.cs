using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// Separates Ridley's supplied colours into the material inputs their paint calculates from and the colours that depart
/// from that calculation: the Ceres fades, Baby, startup, health, alarm and zoom paints and Norfair Ridley's colours.
/// </summary>
internal static class CeresRidleyPaintRecognition
{
    /// <summary>Retains the selected endpoint paint and exceptions to the shared fade formula.</summary>
    internal static CeresRidleyFade Fade(CeresRidleyFadeKind kind, RgbColor[][] shades)
    {
        bool eyes = kind == CeresRidleyFadeKind.Eyes;
        int shadeCount = eyes ? CeresRidleyColorFormat.EyeFadeShadeCount : CeresRidleyPaletteDefinitions.BodyFadeShadeCount;
        int colorCount = eyes ? CeresRidleyColorFormat.EyeFadeColorCount : CeresRidleyColorFormat.BodyFadeColorCount;
        var overrides = new Dictionary<PaintColour, RgbColor>();
        var fade = eyes
            ? new CeresRidleyFade(kind, Eye(shades[0]), bodyPaint: null, overrides)
            : new CeresRidleyFade(kind, default, Body(shades[shadeCount - 1]), overrides);
        for (int shade = 0; shade < shadeCount; shade++)
            for (int color = 0; color < colorCount; color++)
                if (shades[shade][color] != fade.Resolve(shade, color))
                    overrides[new PaintColour(shade, color)] = shades[shade][color];
        return fade;
    }

    /// <summary>Extracts the selected red step and three green levels from the eye endpoint shade.</summary>
    private static CeresRidleyFade.EyePaint Eye(ReadOnlySpan<RgbColor> colors) =>
        new(CeresRidleyFade.EyePaint.MaximumRgb5 - colors[1].FadeLevels.Red, colors[0].FadeLevels.Green, colors[1].FadeLevels.Green, colors[2].FadeLevels.Green);

    /// <summary>Extracts the three membrane endpoints from an eleven-color body palette.</summary>
    private static CeresRidleyBodyPaint Body(ReadOnlySpan<RgbColor> endpoint)
    {
        if (endpoint.Length != 11) throw new ArgumentException("Ridley body endpoint requires eleven colors.", nameof(endpoint));
        return new(endpoint[8], endpoint[9], endpoint[10]);
    }

    /// <summary>Extracts glass, dome, and outline material inputs from the fifteen resting colours.</summary>
    private static CeresBabyMaterial Material(ReadOnlySpan<RgbColor> initial)
    {
        if (initial.Length != CeresRidleyColorFormat.BabyColorCount)
            throw new ArgumentException("Baby paint requires fifteen colors.", nameof(initial));
        return new(initial[0].FadeLevels.Red, initial[0].FadeLevels.Blue, initial[12].FadeLevels.Red, CeresBabyPaintDefinitions.Maximum - initial[12].FadeLevels.Green,
            initial[1].FadeLevels.Red, initial[1].FadeLevels.Green, initial[1].FadeLevels.Green - initial[2].FadeLevels.Green, initial[8].FadeLevels.Red);
    }

    /// <summary>Matches the four Ceres shades to shared body paint while preserving independent color edits.</summary>
    internal static CeresBabyPaint Baby(CeresBabyShadeColours colours, CeresRidleyFade body)
    {
        var edits = new Dictionary<CeresBabyColourCell, RgbColor>();
        var paint = new CeresBabyPaint(CeresBabyPaintShades.Every, Material(colours.Resting), body, standaloneBody: null, edits);
        foreach (CeresBabyShade shade in Enum.GetValues<CeresBabyShade>())
        {
            RgbColor[] supplied = colours.Of(shade);
            for (int color = 0; color < CeresRidleyColorFormat.BabyColorCount; color++)
                if (paint.Resolve(shade, color) != supplied[color]) edits.Add(new CeresBabyColourCell(shade, color), supplied[color]);
        }
        return paint;
    }

    /// <summary>The same material in Norfair Ridley's initial palette, with its own supplied values.</summary>
    private static CeresBabyPaint StandaloneBaby(ReadOnlySpan<RgbColor> initial)
    {
        var edits = new Dictionary<CeresBabyColourCell, RgbColor>();
        CeresBabyMaterial material = Material(initial);
        var paint = new CeresBabyPaint(CeresBabyPaintShades.RestingOnly, material, body: null,
            new CeresRidleyBodyPaint(initial[4], initial[5], initial[6]), edits);
        for (int color = 0; color < CeresRidleyColorFormat.BabyColorCount; color++)
            if (paint.Resolve(CeresBabyShade.Resting, color) != initial[color])
                edits.Add(new CeresBabyColourCell(CeresBabyShade.Resting, color), initial[color]);
        return paint;
    }

    /// <summary>Separates the two startup palettes, preserving colors that differ from calculated source materials.</summary>
    internal static CeresRidleyStartColors Start(ReadOnlySpan<RgbColor> colors, CeresBabyPaint initialBaby)
    {
        if (colors.Length != 2 * CeresRidleyStartColorDefinitions.PaletteSize)
            throw new ArgumentException("Ceres additional palettes require 32 colors and fifteen Baby colors.");
        var edits = new Dictionary<PaintColour, RgbColor>();
        var start = new CeresRidleyStartColors(
            CeresDoorPaintRecognition.Normal(colors.Slice(1, CeresRidleyStartColorDefinitions.PaletteSize - 1)), initialBaby, edits);
        for (int color = 0; color < colors.Length; color++)
            if (start.Resolve(color) != colors[color]) edits.Add(new PaintColour(0, color), colors[color]);
        return start;
    }

    /// <summary>Records only input colors that differ from the calculated healthy-to-membrane tint.</summary>
    internal static CeresRidleyHealthPaint Health(RgbColor[][] bands, CeresRidleyFade body, CeresRidleyFade eyes)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        var health = new CeresRidleyHealthPaint(body, eyes, edits);
        var differing = new Dictionary<PaintColour, RgbColor>();
        for (int band = 0; band < CeresRidleyColorFormat.HealthBandCount; band++)
            for (int color = 0; color < CeresRidleyColorFormat.HealthColorCount; color++)
                if (health.Resolve(band, color) != bands[band][color]) differing.Add(new PaintColour(band, color), bands[band][color]);
        foreach ((PaintColour cell, RgbColor colour) in differing)
            edits.Add(cell, colour);
        return health;
    }

    /// <summary>Compares supplied warning colors with calculated steps and retains only independently edited cells.</summary>
    internal static CeresRidleyAlarmColors Alarm(RgbColor[][] steps)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        var alarm = new CeresRidleyAlarmColors(edits);
        for (int step = 0; step < CeresRidleyColorFormat.AlarmStepCount; step++)
            for (int color = 0; color < CeresRidleyColorFormat.AlarmColorCount; color++)
                if (alarm.Resolve(step, color) != steps[step][color]) edits.Add(new PaintColour(step, color), steps[step][color]);
        return alarm;
    }

    /// <summary>Builds the shared contours from the first zoom step and retains only differences in later steps.</summary>
    internal static CeresRidleyFloorPaint Floor(RgbColor[][] steps)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        var floor = new CeresRidleyFloorPaint(Body(steps[0].AsSpan(0, 11)), Eye(steps[0].AsSpan(11, 3)), edits);
        for (int step = 0; step < CeresRidleyPaletteDefinitions.GetawayZoomStepCount; step++)
            for (int color = 0; color < CeresRidleyColorFormat.GetawayZoomColorCount; color++)
                if (floor.Resolve(step, color) != steps[step][color]) edits.Add(new PaintColour(step, color), steps[step][color]);
        return floor;
    }

    /// <summary>Splits Norfair Ridley's 32 supplied targets into two materials and preserves independent clear-slot edits.</summary>
    internal static NorfairRidleyInitialPaint NorfairInitial(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 32);
        int clearBlue = colors[0].FadeLevels.Blue;
        var clearSlot = new FadeLevelColour(0, 0, clearBlue).ToColour();
        var edits = new Dictionary<PaintColour, RgbColor>();
        if (colors[0] != clearSlot) edits.Add(new PaintColour(0, 0), colors[0]);
        if (colors[16] != clearSlot) edits.Add(new PaintColour(0, 16), colors[16]);
        return new(CeresDoorPaintRecognition.Escape(colors.Slice(1, 15)), StandaloneBaby(colors.Slice(17, 15)), clearBlue, edits);
    }

    /// <summary>Captures only palette cells that differ from the calculated fifteen-step reveal.</summary>
    internal static NorfairRidleyRevealPaint NorfairReveal(RgbColor[][] steps)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        var reveal = new NorfairRidleyRevealPaint(edits);
        for (int step = 0; step < NorfairRidleyRevealPaintDefinitions.Steps; step++)
            for (int color = 0; color < NorfairRidleyRevealPaintDefinitions.Colors; color++)
                if (reveal.ColorAt(step, color) != steps[step][color]) edits.Add(new PaintColour(step, color), steps[step][color]);
        return reveal;
    }
}
