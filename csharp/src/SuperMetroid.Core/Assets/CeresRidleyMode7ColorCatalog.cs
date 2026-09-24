using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres Ridley Mode-7 zoom shades, separate from movement and rotation.</summary>
public sealed class CeresRidleyMode7ColorCatalog
{
    private readonly ushort[][] rows;

    private CeresRidleyMode7ColorCatalog(ushort[][] rows) => this.rows = rows;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Reads an authored color by the high byte of the native zoom word.</summary>
    public ushort Resolve(int zoomHighByte, int color)
    {
        if ((uint)zoomHighByte >= rows.Length)
            throw new ArgumentOutOfRangeException(nameof(zoomHighByte));
        if ((uint)color >= rows[zoomHighByte].Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return rows[zoomHighByte][color];
    }

    public void Apply(SnesCgram cgram, int zoomHighByte)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)zoomHighByte >= rows.Length)
            throw new ArgumentOutOfRangeException(nameof(zoomHighByte));
        for (int color = 0; color < rows[zoomHighByte].Length; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.Mode7ZoomCgramIndex + color,
                rows[zoomHighByte][color]);
    }

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

    public static byte[] Write(CeresRidleyMode7ColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Ceres Ridley Mode-7 property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record CeresRidleyMode7ColorDocument
{
    public required int Version { get; init; }
    /// <summary>Nine zoom-high-byte rows, zero through eight, each with fifteen RGB5 colors.</summary>
    public required PaletteRgb5[][] ZoomRows { get; init; }
}

public static class CeresRidleyMode7ColorFormat
{
    public const string FileName = "ceres-ridley-mode7-colors.json";
    public const int Version = 1;
}
