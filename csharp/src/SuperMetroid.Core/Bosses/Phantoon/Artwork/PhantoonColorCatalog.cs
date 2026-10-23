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
            Span<Bgr555> target = stackalloc Bgr555[PhantoonColorRomData.FadeOutCount];
            for (int color = 0; color < target.Length; color++) target[color] = ResolveFadeOut(color);
            content.AppendColors("fadeOut", target);
            var power = new Bgr555[PhantoonColorRomData.PowerOnCount];
            for (int color = 0; color < power.Length; color++) power[color] = ResolvePowerOn(color);
            content.AppendColors("powerOn", power);
            var frames = new Bgr555[PhantoonColorRomData.HealthBandCount][];
            for (int band = 0; band < frames.Length; band++)
            {
                frames[band] = new Bgr555[PhantoonColorRomData.HealthBandColorCount];
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
    private PhantoonColorCatalog(Bgr555[][] healthBands, Bgr555[] fadeOut, Bgr555[] powerOn)
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
        if ((uint)color >= PhantoonColorRomData.HealthBandColorCount)
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
        if ((uint)color >= PhantoonColorRomData.FadeOutCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return fadeOutEdits.GetValueOrDefault(new ColourCell(0, color));
    }
    /// <summary>$A7:CA61-CB40: all seven powered ship material palettes calculate; supplied words remain independently editable.</summary>
    public Bgr555 ResolvePowerOn(int color)
    {
        if ((uint)color >= PhantoonColorRomData.PowerOnCount) throw new ArgumentOutOfRangeException(nameof(color));
        return powerEdits.TryGetValue(new ColourCell(0, color), out Bgr555 edit) ? edit : WreckedShipPowerPaintDefinitions.Color(color);
    }
    /// <summary>Loads and validates Phantoon's health, fade-out, and ship-power RGB5 sources.</summary>
    /// <param name="json">Caller-owned stream containing the color document.</param>
    /// <returns>The compiled color catalog.</returns>
    public static PhantoonColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        PhantoonColorDocument document;
        document = JsonAssetDocument.Read<PhantoonColorDocument>(json, ArtworkJson.Options, "Phantoon color");
        if (document.Version != PhantoonColorFormat.Version)
            throw new InvalidDataException("Phantoon colors require the supported version.");
        if (document.HealthBands is null ||
            document.HealthBands.Length != PhantoonColorRomData.HealthBandCount)
            throw new InvalidDataException("Phantoon colors require eight health palettes.");
        return new(
            document.HealthBands.Select((band, index) =>
                Compile(band, PhantoonColorRomData.HealthBandColorCount,
                    $"health palette {index}")).ToArray(),
            Compile(document.FadeOut, PhantoonColorRomData.FadeOutCount, "fade-out"),
            Compile(document.PowerOn, PhantoonColorRomData.PowerOnCount, "power-on"));
    }

    /// <summary>Validates and serializes a Phantoon color document as UTF-8 JSON.</summary>
    /// <param name="document">Document containing every required color source.</param>
    /// <returns>A new caller-owned JSON byte array.</returns>
    public static byte[] Write(PhantoonColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

    /// <summary>Rejects a health palette index outside the eight Phantoon bands.</summary>
    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorRomData.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    /// <summary>Checks a palette row's length and five-bit channels before converting it to hardware color words.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Phantoon {name}");

}
