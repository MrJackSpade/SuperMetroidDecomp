using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley Mode-7 zoom shades, separate from movement and rotation.</summary>
public sealed class CeresRidleyMode7ColorCatalog
{
    /// <summary>Immutable decoded shade rows indexed by the native zoom word's high byte.</summary>
    internal readonly CeresRidleyMode7PaintDefinitions rows;

    /// <summary>Copies validated RGB5 rows into the fixed zoom-shade selector.</summary>
    private CeresRidleyMode7ColorCatalog(Bgr555[][] rows) => this.rows = new(rows);

    /// <summary>Uses stable camel-case output and rejects unknown authored color properties.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Reads an authored color by the high byte of the native zoom word.</summary>
    public Bgr555 Resolve(int zoomHighByte, int color)
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
        Ensure.NotNull(cgram);
        _ = Resolve(zoomHighByte, 0);
        for (int color = 0; color < CeresRidleyPaletteRomData.Mode7ZoomColorCount; color++)
            new RidleyPalettes(cgram).CeresMode7Zoom.Write(color, rows.Resolve(zoomHighByte, color));
    }
    /// <summary>Loads all nine zoom shades, each containing fifteen RGB5 colors, rejecting duplicate/unknown JSON properties, unsupported versions, and channel values outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the editable Ceres getaway color document.</param>
    /// <returns>Installed zoom-dependent inks; the native sixteen-word row stride and unused padding are not editable content.</returns>
    /// <exception cref="InvalidDataException">The document schema, row dimensions, or RGB5 colors are invalid.</exception>
    public static CeresRidleyMode7ColorCatalog Load(Stream json)
    {
        FixedGridShape zoomShape = new(CeresRidleyPaletteRomData.Mode7ZoomRowCount,
            CeresRidleyPaletteRomData.Mode7ZoomColorCount);
        Ensure.NotNull(json);
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
            !zoomShape.HasRows(document.ZoomRows))
            throw new InvalidDataException("Ceres Ridley Mode-7 colors require version one and nine zoom rows.");
        var rows = new Bgr555[CeresRidleyPaletteRomData.Mode7ZoomRowCount][];
        for (int row = 0; row < rows.Length; row++)
        {
            PaletteRgb5[]? colors = document.ZoomRows[row];
            if (!zoomShape.HasColumns(colors))
                throw new InvalidDataException($"Ceres Ridley zoom row {row} requires fifteen colors.");
            rows[row] = new Bgr555[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Ceres Ridley zoom row {row} color {color} requires RGB5 channels 0..31.");
                rows[row][color] = rgb.ToBgr555();
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

    /// <summary>Rejects repeated JSON property names before deserialization can silently overwrite a shade.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Ceres Ridley Mode-7 property {name}."));
}
