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
            var power = new RgbColor[PhantoonColorDefinitions.PowerOnCount];
            for (int color = 0; color < power.Length; color++) power[color] = colors.ResolvePowerOn(color);
            content.AppendColors("powerOn", power);
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

    /// <summary>Keeps the powered-ship colours that differ from the calculated paint palette.</summary>
    internal static Dictionary<ColourCell, RgbColor> PowerEdits(RgbColor[] powerOn)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < powerOn.Length; color++)
            if (powerOn[color] != WreckedShipPowerPaint.Color(color)) edits.Add(new ColourCell(0, color), powerOn[color]);
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

    /// <summary>Resolves one powered-ship colour from its edit or the calculated paint.</summary>
    internal static RgbColor PowerOn(Dictionary<ColourCell, RgbColor> edits, int color)
    {
        if ((uint)color >= PhantoonColorDefinitions.PowerOnCount) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(0, color), out RgbColor edit) ? edit : WreckedShipPowerPaint.Color(color);
    }

    /// <summary>Rejects a health palette index outside the eight Phantoon bands.</summary>
    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));
}
