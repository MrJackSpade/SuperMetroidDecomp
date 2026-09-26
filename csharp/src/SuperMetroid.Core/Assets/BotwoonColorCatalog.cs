using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable health-band RGB5 images for Botwoon's sprite palette.</summary>
public sealed class BotwoonColorCatalog
{
    private readonly ushort[][] health;

    private BotwoonColorCatalog(ushort[][] health) => this.health = health;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort HealthColor(int band, int color) =>
        (uint)band < health.Length &&
        (uint)color < BotwoonHealthPaletteDefinitions.ColorsPerPalette
            ? health[band][color]
            : throw new ArgumentOutOfRangeException(nameof(band),
                $"Botwoon health band {band}, color {color} is outside the authored images.");

    public static BotwoonColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        BotwoonColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<BotwoonColorDocument>(JsonOptions) ??
                throw new InvalidDataException("Botwoon color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Botwoon color JSON.", error);
        }
        if (document.Version != BotwoonColorFormat.Version)
            throw new InvalidDataException("Botwoon colors require the supported version.");
        if (document.Health is null ||
            document.Health.Length != BotwoonHealthPaletteDefinitions.PaletteCount)
            throw new InvalidDataException("Botwoon colors require eight health bands.");
        return new(document.Health.Select((frame, index) =>
            Compile(frame, index)).ToArray());
    }

    public static byte[] Write(BotwoonColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, int band)
    {
        if (source is null ||
            source.Length != BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new InvalidDataException(
                $"Botwoon health band {band} requires sixteen RGB5 colors.");
        var compiled = new ushort[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Botwoon health band {band} color {color} requires RGB5 channels 0..31.");
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
                        $"Duplicate Botwoon color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record BotwoonColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Health { get; init; }
}

public static class BotwoonColorFormat
{
    public const string FileName = "botwoon-colors.json";
    public const int Version = 1;
}
