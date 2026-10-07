using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable RGB5 color targets for Phantoon's health, materialization and ship-power
/// transitions. Boss phase selection and interpolation arithmetic remain compiled.
/// </summary>
public sealed class PhantoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("PhantoonColorCatalog-v1", content =>
        {
            Span<ushort> target = stackalloc ushort[PhantoonColorRomData.FadeOutCount];
            for (int color = 0; color < target.Length; color++) target[color] = ResolveFadeOut(color);
            content.AppendWords("fadeOut", target);
            var power = new ushort[PhantoonColorRomData.PowerOnCount];
            for (int color = 0; color < power.Length; color++) power[color] = ResolvePowerOn(color);
            content.AppendWords("powerOn", power);
            var frames = new ushort[PhantoonColorRomData.HealthBandCount][];
            for (int band = 0; band < frames.Length; band++)
            {
                frames[band] = new ushort[PhantoonColorRomData.HealthBandColorCount];
                for (int color = 0; color < frames[band].Length; color++) frames[band][color] = ResolveHealth(band, color);
            }
            content.AppendWordFrames("healthBands", frames);
        });

    private readonly Dictionary<int, ushort> healthEdits = [];
    private readonly Dictionary<int, ushort> fadeOutEdits = [];
    private readonly Dictionary<int, ushort> powerEdits = [];

    private PhantoonColorCatalog(ushort[][] healthBands, ushort[] fadeOut, ushort[] powerOn)
    {
        for (int band = 0; band < healthBands.Length; band++)
        for (int color = 0; color < healthBands[band].Length; color++)
            if (healthBands[band][color] != PhantoonHealthPaintDefinitions.Color(band, color))
                healthEdits.Add(band * PhantoonColorRomData.HealthBandColorCount + color, healthBands[band][color]);
        for (int color = 0; color < fadeOut.Length; color++)
            if (fadeOut[color] != 0) fadeOutEdits.Add(color, fadeOut[color]);
        for (int color = 0; color < powerOn.Length; color++)
            if (powerOn[color] != WreckedShipPowerPaintDefinitions.Color(color)) powerEdits.Add(color, powerOn[color]);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>
    /// $A7:CB41..CC40 health targets, selected by $A7:DC0F. All healthy material
    /// shades and red-tinted bands calculate; supplied edits remain independent
    /// across every ink and band, including the healthy endpoints.
    /// </summary>
    public ushort ResolveHealth(int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= PhantoonColorRomData.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = band * PhantoonColorRomData.HealthBandColorCount + color;
        return healthEdits.TryGetValue(key, out ushort edit) ? edit : PhantoonHealthPaintDefinitions.Color(band, color);
    }
    /// <summary>
    /// $A7:CA41..CA60, Palette_Phantoon_FadeOutTarget: $A7:DBB1 fades every
    /// body palette component to black. Independent supplied target edits override it.
    /// </summary>
    public ushort ResolveFadeOut(int color)
    {
        if ((uint)color >= PhantoonColorRomData.FadeOutCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return fadeOutEdits.GetValueOrDefault(color);
    }
    /// <summary>$A7:CA61-CB40: all seven powered ship material palettes calculate; supplied words remain independently editable.</summary>
    public ushort ResolvePowerOn(int color)
    {
        if ((uint)color >= PhantoonColorRomData.PowerOnCount) throw new ArgumentOutOfRangeException(nameof(color));
        return powerEdits.TryGetValue(color, out ushort edit) ? edit : WreckedShipPowerPaintDefinitions.Color(color);
    }
    public static PhantoonColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        PhantoonColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<PhantoonColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Phantoon color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Phantoon color JSON.", error);
        }
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

    public static byte[] Write(PhantoonColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorRomData.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));


    private static ushort[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Phantoon {name} requires {count} RGB5 colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Phantoon {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Phantoon color property {name}."));
}

public sealed record PhantoonColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] HealthBands { get; init; }
    public required PaletteRgb5[] FadeOut { get; init; }
    public required PaletteRgb5[] PowerOn { get; init; }
}

public static class PhantoonColorFormat
{
    public const string FileName = "phantoon-colors.json";
    public const int Version = 1;
}
