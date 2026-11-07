using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>
/// Compares Phantoon's supplied health, fade-out and ship-power colours with the calculated paint and keeps only the
/// colours that differ, the selection the game resolves its colours from.
/// </summary>
internal static class PhantoonColorEdits
{
    /// <summary>Selects the authored deviations from the calculated health, fade and ship-power colours.</summary>
    /// <param name="healthBands">The health palettes' colours.</param>
    /// <param name="fadeOut">The materialization fade-out target.</param>
    /// <param name="powerOn">Each background set the power-on fades, with its sixteen target colours.</param>
    internal static PhantoonColorSelection Select(RgbColor[][] healthBands, RgbColor[] fadeOut,
        IReadOnlyDictionary<WreckedShipPoweredPart, RgbColor[]> powerOn) =>
        new(HealthEdits(healthBands), FadeOutEdits(fadeOut), PowerEdits(powerOn));

    /// <summary>Keeps the health-band colours that differ from the calculated material and damage shades.</summary>
    private static Dictionary<PaintColour, RgbColor> HealthEdits(RgbColor[][] healthBands)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        for (int band = 0; band < healthBands.Length; band++)
        for (int color = 0; color < healthBands[band].Length; color++)
            if (healthBands[band][color] != PhantoonHealthPaint.Color(band, color))
                edits.Add(new PaintColour(band, color), healthBands[band][color]);
        return edits;
    }

    /// <summary>Keeps the nonblack fade-out targets.</summary>
    private static Dictionary<PaintColour, RgbColor> FadeOutEdits(RgbColor[] fadeOut)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        for (int color = 0; color < fadeOut.Length; color++)
            if (fadeOut[color] != RgbColor.Black) edits.Add(new PaintColour(0, color), fadeOut[color]);
        return edits;
    }

    /// <summary>
    /// Keeps the powered-ship colours that differ from the calculated paint, each under the part it paints. Every part the
    /// power-on fades must be supplied with all of its colours, and no other part.
    /// </summary>
    private static Dictionary<WreckedShipPoweredColour, RgbColor> PowerEdits(IReadOnlyDictionary<WreckedShipPoweredPart, RgbColor[]> powerOn)
    {
        Ensure.NotNull(powerOn);
        if (powerOn.Count != PhantoonColorFormat.PoweredParts.Length)
            throw new ArgumentException("The power-on targets must name exactly the parts the power-on fades.", nameof(powerOn));
        var edits = new Dictionary<WreckedShipPoweredColour, RgbColor>();
        foreach (WreckedShipPoweredPart part in PhantoonColorFormat.PoweredParts)
        {
            RgbColor[] colours = powerOn.TryGetValue(part, out RgbColor[]? supplied)
                ? supplied
                : throw new ArgumentException($"The power-on targets have no colours for {part}.", nameof(powerOn));
            if (colours.Length != WreckedShipPowerPaintDefinitions.PartColourCount)
                throw new ArgumentException($"The power-on targets of {part} must hold one group's colours.", nameof(powerOn));
            for (int color = 0; color < colours.Length; color++)
                if (colours[color] != WreckedShipPowerPaint.Color(part, color))
                    edits.Add(new WreckedShipPoweredColour(part, color), colours[color]);
        }
        return edits;
    }
}
