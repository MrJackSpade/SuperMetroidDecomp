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
    private readonly ushort[][] healthBands;
    private readonly ushort[] fadeOut;
    private readonly ushort[] powerOn;

    private PhantoonColorCatalog(ushort[][] healthBands, ushort[] fadeOut, ushort[] powerOn)
    {
        this.healthBands = healthBands;
        this.fadeOut = fadeOut;
        this.powerOn = powerOn;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveHealth(int band, int color) =>
        Get(healthBands[CheckBand(band)], color);
    public ushort ResolveFadeOut(int color) => Get(fadeOut, color);
    public ushort ResolvePowerOn(int color) => Get(powerOn, color);

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

    private static ushort Get(ushort[] colors, int index) =>
        (uint)index < colors.Length
            ? colors[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

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

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Phantoon color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
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
