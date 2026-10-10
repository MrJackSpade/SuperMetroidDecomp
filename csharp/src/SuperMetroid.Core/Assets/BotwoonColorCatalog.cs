using SuperMetroid.Core.Hardware;
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
            Span<Bgr555> row = stackalloc Bgr555[BotwoonHealthPaletteDefinitions.ColorsPerPalette];
            for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = HealthColor(band, color);
                content.AppendColors("row", row);
            }
        });

    private readonly BotwoonHealthPaintDefinitions health;

    private BotwoonColorCatalog(Bgr555[][] rows) => health = new(rows);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Gets one compiled BGR555 word from an independently editable health-band palette.</summary>
    /// <param name="band">Health-band index from zero through seven.</param>
    /// <param name="color">Color index from zero through fifteen within the complete palette.</param>
    /// <returns>The selected packed SNES color word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    public Bgr555 HealthColor(int band, int color)
    {
        if ((uint)band >= BotwoonHealthPaletteDefinitions.PaletteCount ||
            (uint)color >= BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(band),
                $"Botwoon health band {band}, color {color} is outside the authored images.");
        return health.Resolve(band, color);
    }

    /// <summary>Loads and validates all eight complete Botwoon health palettes from editable RGB5 JSON.</summary>
    /// <param name="json">The caller-owned stream containing the color document.</param>
    /// <returns>The compiled immutable palette catalog.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette dimensions, or RGB5 channels are invalid.</exception>
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

    /// <summary>Serializes a Botwoon color document and validates the resulting JSON through <see cref="Load"/>.</summary>
    /// <param name="document">The eight complete RGB5 health palettes to serialize.</param>
    /// <returns>Validated UTF-8 JSON bytes.</returns>
    public static byte[] Write(BotwoonColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static Bgr555[] Compile(PaletteRgb5[]? source, int band)
    {
        if (source is null ||
            source.Length != BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new InvalidDataException(
                $"Botwoon health band {band} requires sixteen RGB5 colors.");
        var compiled = new Bgr555[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Botwoon health band {band} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Botwoon color property {name}."));
}

/// <summary>Editable JSON schema for Botwoon's eight health-dependent sprite-palette images.</summary>
public sealed record BotwoonColorDocument
{
    /// <summary>Gets the Botwoon color schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets eight ordered rows of sixteen RGB5 colors corresponding to native <c>$B3:971B-$981A</c>.</summary>
    public required PaletteRgb5[][] Health { get; init; }
}

/// <summary>Names and versions the editable Botwoon health-color asset.</summary>
public static class BotwoonColorFormat
{
    /// <summary>The embedded Botwoon color asset file name.</summary>
    public const string FileName = "botwoon-colors.json";

    /// <summary>The supported Botwoon color JSON schema version.</summary>
    public const int Version = 1;
}
