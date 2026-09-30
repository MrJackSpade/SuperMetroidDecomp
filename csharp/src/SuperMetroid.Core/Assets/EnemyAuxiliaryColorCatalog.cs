using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed RGB5 animation rows independent of cartridge storage and enemy mechanics.</summary>
public sealed class EnemyAuxiliaryColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-auxiliary-colors-v1", content =>
        {
            foreach ((EnemyAuxiliaryPalette palette, ushort[][] rows) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("palette", (int)palette);
                content.AppendWordFrames("frames", rows);
            }
        });

    private readonly Dictionary<EnemyAuxiliaryPalette, ushort[][]> frames;
    private EnemyAuxiliaryColorCatalog(Dictionary<EnemyAuxiliaryPalette, ushort[][]> frames) => this.frames = frames;

    public ushort Resolve(EnemyAuxiliaryPalette palette, int frame, int color)
    {
        if (!frames.TryGetValue(palette, out ushort[][]? rows)) throw new ArgumentOutOfRangeException(nameof(palette));
        if ((uint)frame >= rows.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= rows[frame].Length) throw new ArgumentOutOfRangeException(nameof(color));
        return rows[frame][color];
    }

    public static EnemyAuxiliaryColorCatalog Load(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        EnemyAuxiliaryColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(source);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyAuxiliaryColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Enemy auxiliary colors are null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid enemy auxiliary color JSON.", error); }
        if (document.Version != EnemyAuxiliaryColorFormat.Version || document.Palettes is null ||
            document.Palettes.Count != EnemyAuxiliaryColorDefinitions.All.Length)
            throw new InvalidDataException("Enemy auxiliary colors require version one and all four named palettes.");
        var result = new Dictionary<EnemyAuxiliaryPalette, ushort[][]>();
        foreach (EnemyAuxiliaryPaletteDefinition definition in EnemyAuxiliaryColorDefinitions.All)
        {
            if (!document.Palettes.TryGetValue(definition.Id, out PaletteRgb5[][]? rows) ||
                rows is null || rows.Length != definition.FrameCount)
                throw new InvalidDataException($"Palette {definition.Id} requires {definition.FrameCount} frames.");
            var compiled = new ushort[rows.Length][];
            for (int frame = 0; frame < rows.Length; frame++)
            {
                PaletteRgb5[]? row = rows[frame];
                if (row is null || row.Length != definition.ColorCount)
                    throw new InvalidDataException($"Palette {definition.Id} frame {frame} requires {definition.ColorCount} colors.");
                compiled[frame] = new ushort[row.Length];
                for (int color = 0; color < row.Length; color++)
                {
                    PaletteRgb5? rgb = row[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Palette {definition.Id} frame {frame} color {color} requires RGB5 channels 0..31.");
                    compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                }
            }
            result.Add(definition.Id, compiled);
        }
        return new EnemyAuxiliaryColorCatalog(result);
    }

    public static byte[] Write(EnemyAuxiliaryColorDocument document)
    {
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(data, writable: false));
        return data;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new InvalidDataException($"Duplicate auxiliary palette property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter<EnemyAuxiliaryPalette>(allowIntegerValues: false) },
    };
}

public sealed record EnemyAuxiliaryColorDocument
{
    public required int Version { get; init; }
    public required Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]> Palettes { get; init; }
}
