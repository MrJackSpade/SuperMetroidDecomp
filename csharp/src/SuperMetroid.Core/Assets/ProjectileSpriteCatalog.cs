using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable, ROM-independent projectile visual compositions; no damage, collision or timing fields.</summary>
public sealed class ProjectileSpriteCatalog
{
    private readonly Dictionary<ushort, CompiledSpritePart[]> frames;
    private ProjectileSpriteCatalog(Dictionary<ushort, CompiledSpritePart[]> frames) => this.frames = frames;

    public void Draw(ushort id, OamBuffer oam, ushort x, ushort y)
    {
        if (!frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id:X4}.");
        foreach (var part in parts) oam.AddProjectileSpritePart(part.X, part.Y, part.Attributes, x, y);
    }

    public static ProjectileSpriteCatalog Load(Stream json)
    {
        ProjectileSpriteDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ProjectileSpriteDocument>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new InvalidDataException("Projectile composition document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid projectile composition JSON.", error); }
        if (document.Version != ProjectileSpriteDefinitions.Version || document.Frames is null || document.Frames.Count != ProjectileSpriteDefinitions.NativePointers.Length)
            throw new InvalidDataException("Projectile compositions require version 1 and every timed-projectile sprite.");
        var frames = new Dictionary<ushort, CompiledSpritePart[]>();
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        {
            string name = ProjectileSpriteDefinitions.Name(id);
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > ProjectileSpriteDefinitions.MaximumParts)
                throw new InvalidDataException($"Projectile sprite {name} is missing or exceeds OAM capacity.");
            var compiled = new CompiledSpritePart[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i];
                if (p is null || p.OffsetX is < -256 or > 255 || p.OffsetY is < -128 or > 127 ||
                    p.Size is not (8 or 16) || p.Priority is < 0 or > 3 || p.Palette is null or < 0 or > 7 ||
                    p.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || p.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows)
                    throw new InvalidDataException($"Projectile sprite {name} part {i} has invalid visual fields.");
                // Hardware wraps a large part's adjacent tile at row/sheet boundaries.
                // Do not impose the menu atlas's contiguous-rectangle restriction here.
                var attributes = SnesObjAttributeWord.Create(p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                    p.Palette.Value, p.Priority, (p.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (p.FlipY ? SnesTileFlipFlags.Vertical : 0));
                compiled[i] = new(SnesSpritemapXWord.Create(p.OffsetX, p.Size == 16), unchecked((byte)(sbyte)p.OffsetY), attributes, false);
            }
            frames.Add(id, compiled);
        }
        return new(frames);
    }

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new InvalidDataException($"Duplicate projectile composition property {property.Name}.");
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) RejectDuplicateProperties(item);
    }
}

public sealed record ProjectileSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
