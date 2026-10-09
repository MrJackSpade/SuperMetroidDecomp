using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres Ridley Mode-7 zoom shades, separate from movement and rotation.</summary>
public sealed class CeresRidleyMode7ColorCatalog
{
    /// <summary>Packed RGB5 colors grouped by the high byte of the native Mode-7 zoom word.</summary>
    private readonly CeresRidleyMode7PaintDefinitions rows;

    /// <summary>Creates a catalog over the decoded zoom-shade rows.</summary>
    /// <param name="rows">Per-zoom color values packed in SNES CGRAM word format.</param>
    private CeresRidleyMode7ColorCatalog(ushort[][] rows) => this.rows = new(rows);

    /// <summary>JSON settings that enforce camel-case schema names, reject unmapped properties, and produce indented output.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Reads an authored color by the high byte of the native zoom word.</summary>
    public ushort Resolve(int zoomHighByte, int color)
    {
        if ((uint)zoomHighByte >= CeresRidleyPaletteRomData.Mode7ZoomRowCount)
            throw new ArgumentOutOfRangeException(nameof(zoomHighByte));
        return rows.Resolve(zoomHighByte, color);
    }

    /// <summary>Reproduces $A6:B0EF's fifteen-color zoom shade write to CGRAM 81..95, preserving BG palette 5 color zero and leaving zoom/rotation mechanics unchanged.</summary>
    /// <param name="cgram">Destination palette memory.</param>
    /// <param name="zoomHighByte">High byte of the current native zoom word, 0..8; not a frame timer or byte offset into the stored rows.</param>
    /// <exception cref="ArgumentOutOfRangeException">The zoom row is outside 0..8.</exception>
    public void Apply(SnesCgram cgram, int zoomHighByte)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        _ = Resolve(zoomHighByte, 0);
        for (int color = 0; color < CeresRidleyPaletteRomData.Mode7ZoomColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.Mode7ZoomCgramIndex + color, rows.Resolve(zoomHighByte, color));
    }
    /// <summary>Loads all nine zoom shades, each containing fifteen RGB5 colors, rejecting duplicate/unknown JSON properties, unsupported versions, and channel values outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the editable Ceres getaway color document.</param>
    /// <returns>Installed zoom-dependent inks; the native sixteen-word row stride and unused padding are not editable content.</returns>
    /// <exception cref="InvalidDataException">The document schema, row dimensions, or RGB5 colors are invalid.</exception>
    public static CeresRidleyMode7ColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresRidleyMode7ColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresRidleyMode7ColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Ceres Ridley Mode-7 colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres Ridley Mode-7 color JSON.", error);
        }
        if (document.Version != CeresRidleyMode7ColorFormat.Version ||
            document.ZoomRows is null ||
            document.ZoomRows.Length != CeresRidleyPaletteRomData.Mode7ZoomRowCount)
            throw new InvalidDataException("Ceres Ridley Mode-7 colors require version one and nine zoom rows.");
        var rows = new ushort[CeresRidleyPaletteRomData.Mode7ZoomRowCount][];
        for (int row = 0; row < rows.Length; row++)
        {
            PaletteRgb5[]? colors = document.ZoomRows[row];
            if (colors is null || colors.Length != CeresRidleyPaletteRomData.Mode7ZoomColorCount)
                throw new InvalidDataException($"Ceres Ridley zoom row {row} requires fifteen colors.");
            rows[row] = new ushort[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Ceres Ridley zoom row {row} color {color} requires RGB5 channels 0..31.");
                rows[row][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new(rows);
    }

    /// <summary>Serializes all zoom shades as UTF-8 JSON and validates them through <see cref="Load"/> before returning the payload.</summary>
    /// <param name="document">Complete nine-row color document to validate and serialize.</param>
    /// <returns>Validated JSON bytes, without writing an external resource.</returns>
    public static byte[] Write(CeresRidleyMode7ColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Rejects duplicate JSON object properties before deserialization using ordinal name comparison.</summary>
    /// <param name="value">Root JSON value to scan for repeated properties.</param>
    /// <exception cref="InvalidDataException">An object contains a property name more than once.</exception>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Ceres Ridley Mode-7 property {name}."));
}

/// <summary>Editable Ceres Ridley getaway zoom shades corresponding to $A6:B107 onward, excluding each native row's unused sixteenth word.</summary>
public sealed record CeresRidleyMode7ColorDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresRidleyMode7ColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nine zoom-high-byte rows, zero through eight, each with fifteen RGB5 colors.</summary>
    public required PaletteRgb5[][] ZoomRows { get; init; }
}

/// <summary>Editable color-resource filename and JSON revision, separate from native zoom-state and palette-destination definitions.</summary>
public static class CeresRidleyMode7ColorFormat
{
    /// <summary>JSON filename loaded by the presentation catalog for the nine Mode-7 getaway shades.</summary>
    public const string FileName = "ceres-ridley-mode7-colors.json";
    /// <summary>Supported revision of the nine-by-fifteen RGB5 zoom-shade schema.</summary>
    public const int Version = 1;
}
