using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Editable RGB5 color targets for Phantoon's health, materialization and ship-power
/// transitions. Boss phase selection and interpolation arithmetic remain compiled.
/// </summary>
public sealed class PhantoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("PhantoonColorCatalog-v1", content =>
        {
            Span<Bgr555> target = stackalloc Bgr555[PhantoonColorDefinitions.FadeOutCount];
            for (int color = 0; color < target.Length; color++) target[color] = ResolveFadeOut(color);
            content.AppendColors("fadeOut", target);
            var power = new Bgr555[PhantoonColorDefinitions.PowerOnCount];
            for (int color = 0; color < power.Length; color++) power[color] = ResolvePowerOn(color);
            content.AppendColors("powerOn", power);
            var frames = new Bgr555[PhantoonColorDefinitions.HealthBandCount][];
            for (int band = 0; band < frames.Length; band++)
            {
                frames[band] = new Bgr555[PhantoonColorDefinitions.HealthBandColorCount];
                for (int color = 0; color < frames[band].Length; color++) frames[band][color] = ResolveHealth(band, color);
            }
            content.AppendColorFrames("healthBands", frames);
        });

    /// <summary>Health-band colors that differ from the calculated material and damage shades.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> healthEdits = [];
    /// <summary>Nonblack targets supplied for Phantoon's fade-out palette.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> fadeOutEdits = [];
    /// <summary>Powered-ship colors that differ from the calculated paint palette.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> powerEdits = [];

    /// <summary>Retains only authored deviations from the calculated health, fade, and ship-power colors.</summary>
    internal PhantoonColorCatalog(Bgr555[][] healthBands, Bgr555[] fadeOut, Bgr555[] powerOn)
    {
        for (int band = 0; band < healthBands.Length; band++)
        for (int color = 0; color < healthBands[band].Length; color++)
            if (healthBands[band][color] != PhantoonHealthPaintDefinitions.Color(band, color))
                healthEdits.Add(new ColourCell(band, color), healthBands[band][color]);
        for (int color = 0; color < fadeOut.Length; color++)
            if (fadeOut[color] != Bgr555.Black) fadeOutEdits.Add(new ColourCell(0, color), fadeOut[color]);
        for (int color = 0; color < powerOn.Length; color++)
            if (powerOn[color] != WreckedShipPowerPaintDefinitions.Color(color)) powerEdits.Add(new ColourCell(0, color), powerOn[color]);
    }

    /// <summary>
    /// $A7:CB41..CC40 health targets, selected by $A7:DC0F. All healthy material
    /// shades and red-tinted bands calculate; supplied edits remain independent
    /// across every ink and band, including the healthy endpoints.
    /// </summary>
    public Bgr555 ResolveHealth(int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= PhantoonColorDefinitions.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(band, color);
        return healthEdits.TryGetValue(key, out Bgr555 edit) ? edit : PhantoonHealthPaintDefinitions.Color(band, color);
    }
    /// <summary>
    /// $A7:CA41..CA60, Palette_Phantoon_FadeOutTarget: $A7:DBB1 fades every
    /// body palette component to black. Independent supplied target edits override it.
    /// </summary>
    public Bgr555 ResolveFadeOut(int color)
    {
        if ((uint)color >= PhantoonColorDefinitions.FadeOutCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return fadeOutEdits.GetValueOrDefault(new ColourCell(0, color));
    }
    /// <summary>$A7:CA61-CB40: all seven powered ship material palettes calculate; supplied words remain independently editable.</summary>
    public Bgr555 ResolvePowerOn(int color)
    {
        if ((uint)color >= PhantoonColorDefinitions.PowerOnCount) throw new ArgumentOutOfRangeException(nameof(color));
        return powerEdits.TryGetValue(new ColourCell(0, color), out Bgr555 edit) ? edit : WreckedShipPowerPaintDefinitions.Color(color);
    }

    /// <summary>Rejects a health palette index outside the eight Phantoon bands.</summary>
    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));
}
