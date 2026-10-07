using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable health-band RGB5 images for Botwoon's sprite palette.</summary>
public sealed class BotwoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("BotwoonColorCatalog-v1", content =>
        {
            content.Append("health", BotwoonHealthPaletteDefinitions.PaletteCount);
            Span<ushort> row = stackalloc ushort[BotwoonHealthPaletteDefinitions.ColorsPerPalette];
            for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = HealthColor(band, color);
                content.AppendWords("row", row);
            }
        });

    private readonly BotwoonHealthPaintDefinitions health;

    private BotwoonColorCatalog(ushort[][] rows) => health = new(rows);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort HealthColor(int band, int color)
    {
        if ((uint)band >= BotwoonHealthPaletteDefinitions.PaletteCount ||
            (uint)color >= BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(band),
                $"Botwoon health band {band}, color {color} is outside the authored images.");
        return health.Resolve(band, color);
    }

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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Botwoon color property {name}."));
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
