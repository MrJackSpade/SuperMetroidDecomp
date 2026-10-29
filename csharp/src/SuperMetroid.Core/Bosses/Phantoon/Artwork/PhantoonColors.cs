using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's installed colours: which supplied health, fade-out and ship-power colours are edits of the calculated
/// paint, how each resolves, and the identity of all three.
/// </summary>
internal static class PhantoonColors
{
    /// <summary>Hashes the fade-out and power-on targets, then every health band.</summary>
    internal static string ContentIdentity(PhantoonColorCatalog colors) =>
        SelectedPresentationHash.Create("PhantoonColorCatalog-v1", content =>
        {
            Span<RgbColor> target = stackalloc RgbColor[PhantoonColorDefinitions.FadeOutCount];
            for (int color = 0; color < target.Length; color++) target[color] = colors.ResolveFadeOut(color);
            content.AppendColors("fadeOut", target);
            var power = new List<RgbColor>(PhantoonColorDefinitions.PowerOnSets.Length * ColourSets.SetColours);
            foreach (BackgroundColourSet set in PhantoonColorDefinitions.PowerOnSets)
                for (int color = 0; color < ColourSets.SetColours; color++) power.Add(colors.ResolvePowerOn(set, color));
            content.AppendColors("powerOn", power.ToArray());
            var frames = new RgbColor[PhantoonColorDefinitions.HealthBandCount][];
            for (int band = 0; band < frames.Length; band++)
            {
                frames[band] = new RgbColor[PhantoonColorDefinitions.HealthBandColorCount];
                for (int color = 0; color < frames[band].Length; color++) frames[band][color] = colors.ResolveHealth(band, color);
            }
            content.AppendColorFrames("healthBands", frames);
        });

    /// <summary>Keeps the health-band colours that differ from the calculated material and damage shades.</summary>
    internal static Dictionary<ColourCell, RgbColor> HealthEdits(RgbColor[][] healthBands)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int band = 0; band < healthBands.Length; band++)
        for (int color = 0; color < healthBands[band].Length; color++)
            if (healthBands[band][color] != PhantoonHealthPaint.Color(band, color))
                edits.Add(new ColourCell(band, color), healthBands[band][color]);
        return edits;
    }

    /// <summary>Keeps the nonblack fade-out targets.</summary>
    internal static Dictionary<ColourCell, RgbColor> FadeOutEdits(RgbColor[] fadeOut)
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
    internal static Dictionary<BackgroundSetColour, RgbColor> PowerEdits(IReadOnlyDictionary<BackgroundColourSet, RgbColor[]> powerOn)
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

    /// <summary>Resolves one health-band colour from its edit or the calculated shade.</summary>
    internal static RgbColor Health(Dictionary<ColourCell, RgbColor> edits, int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= PhantoonColorDefinitions.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(band, color);
        return edits.TryGetValue(key, out RgbColor edit) ? edit : PhantoonHealthPaint.Color(band, color);
    }

    /// <summary>Resolves one fade-out target: its edit, or black.</summary>
    internal static RgbColor FadeOut(Dictionary<ColourCell, RgbColor> edits, int color)
    {
        if ((uint)color >= PhantoonColorDefinitions.FadeOutCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.GetValueOrDefault(new ColourCell(0, color));
    }

    /// <summary>Resolves one powered-ship colour of a faded set from its edit or the calculated paint.</summary>
    internal static RgbColor PowerOn(Dictionary<BackgroundSetColour, RgbColor> edits, BackgroundColourSet set, int color)
    {
        if (!PhantoonColorDefinitions.PowerOnSets.Contains(set))
            throw new ArgumentOutOfRangeException(nameof(set), set, "The Wrecked Ship's power-on does not fade this set.");
        if ((uint)color >= ColourSets.SetColours) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new BackgroundSetColour(set, color), out RgbColor edit) ? edit : WreckedShipPowerPaint.Color(set, color);
    }

    /// <summary>Rejects a health palette index outside the eight Phantoon bands.</summary>
    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));
}
