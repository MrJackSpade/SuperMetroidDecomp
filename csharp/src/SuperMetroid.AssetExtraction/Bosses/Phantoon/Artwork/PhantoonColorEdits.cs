using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
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
        IReadOnlyDictionary<BackgroundColourSet, RgbColor[]> powerOn) =>
        new(HealthEdits(healthBands), FadeOutEdits(fadeOut), PowerEdits(powerOn));

    /// <summary>Keeps the health-band colours that differ from the calculated material and damage shades.</summary>
    private static Dictionary<ColourCell, RgbColor> HealthEdits(RgbColor[][] healthBands)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int band = 0; band < healthBands.Length; band++)
        for (int color = 0; color < healthBands[band].Length; color++)
            if (healthBands[band][color] != PhantoonHealthPaint.Color(band, color))
                edits.Add(new ColourCell(band, color), healthBands[band][color]);
        return edits;
    }

    /// <summary>Keeps the nonblack fade-out targets.</summary>
    private static Dictionary<ColourCell, RgbColor> FadeOutEdits(RgbColor[] fadeOut)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < fadeOut.Length; color++)
            if (fadeOut[color] != RgbColor.Black) edits.Add(new ColourCell(0, color), fadeOut[color]);
        return edits;
    }

    /// <summary>
    /// Keeps the powered-ship colours that differ from the calculated paint, each under the background set it targets.
    /// Every set the power-on fades must be supplied with all of its colours, and no other set.
    /// </summary>
    private static Dictionary<BackgroundSetColour, RgbColor> PowerEdits(IReadOnlyDictionary<BackgroundColourSet, RgbColor[]> powerOn)
    {
        Ensure.NotNull(powerOn);
        if (powerOn.Count != PhantoonColorDefinitions.PowerOnSets.Length)
            throw new ArgumentException("The power-on targets must name exactly the sets the power-on fades.", nameof(powerOn));
        var edits = new Dictionary<BackgroundSetColour, RgbColor>();
        foreach (BackgroundColourSet set in PhantoonColorDefinitions.PowerOnSets)
        {
            RgbColor[] colours = powerOn.TryGetValue(set, out RgbColor[]? supplied)
                ? supplied
                : throw new ArgumentException($"The power-on targets have no colours for {set}.", nameof(powerOn));
            if (colours.Length != ColourSets.SetColours)
                throw new ArgumentException($"The power-on targets of {set} must hold one set's colours.", nameof(powerOn));
            for (int color = 0; color < colours.Length; color++)
                if (colours[color] != WreckedShipPowerPaint.Color(set, color))
                    edits.Add(new BackgroundSetColour(set, color), colours[color]);
        }
        return edits;
    }
}
