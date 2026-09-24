using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    private readonly Dictionary<int, EnemySpritemapPart[]> frames;

    private EnemySpritemapCatalog(Dictionary<int, EnemySpritemapPart[]> frames) =>
        this.frames = frames;

    /// <summary>Returns a known installed frame; an unrelated enemy may still use its ROM path.</summary>
    public bool TryGet(byte bank, ushort pointer, out ReadOnlyMemory<EnemySpritemapPart> parts)
    {
        if (frames.TryGetValue((bank << 16) | pointer, out EnemySpritemapPart[]? found))
        {
            parts = found;
            return true;
        }
        parts = default;
        return false;
    }

    /// <summary>Validates every authored frame and compiles visual-only fields into OAM parts.</summary>
    public static EnemySpritemapCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EnemySpritemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemySpritemapDocument>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new InvalidDataException("Enemy composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy composition JSON.", error);
        }
        if (document.Version != EnemySpritemapDefinitions.Version ||
            document.Frames is null ||
            document.Frames.Count != EnemySpritemapDefinitions.Frames.Length)
            throw new InvalidDataException(
                "Enemy compositions require version one and every named frame.");

        var frames = new Dictionary<int, EnemySpritemapPart[]>();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            if (!document.Frames.TryGetValue(frame.Name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemySpritemapDefinitions.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} is missing or exceeds OAM capacity.");
            var parts = new EnemySpritemapPart[visual.Length];
            for (int index = 0; index < parts.Length; index++)
            {
                SpriteVisualPart? part = visual[index];
                if (part is null || part.OffsetX is < -256 or > 255 ||
                    part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                    part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                    part.TileColumn is < 0 or >= EnemySpritemapDefinitions.TileColumns ||
                    part.TileRow is < 0 or >= EnemySpritemapDefinitions.TileRows)
                    throw new InvalidDataException(
                        $"Enemy composition {frame.Name} part {index} has invalid visual fields.");
                SnesTileFlipFlags flips =
                    (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
                parts[index] = new EnemySpritemapPart(
                    SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                    unchecked((byte)(sbyte)part.OffsetY),
                    SnesObjAttributeWord.Create(
                        part.TileRow * EnemySpritemapDefinitions.TileColumns + part.TileColumn,
                        part.Palette.Value, part.Priority, flips));
            }
            if (!frames.TryAdd((frame.Bank << 16) | frame.Pointer, parts))
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} repeats a visual identity.");
        }
        return new EnemySpritemapCatalog(frames);
    }

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate enemy composition property {property.Name}.");
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (JsonElement item in element.EnumerateArray())
                RejectDuplicateProperties(item);
    }
}

/// <summary>Versioned, semantic enemy frame names mapped to editable OAM parts.</summary>
public sealed record EnemySpritemapDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
