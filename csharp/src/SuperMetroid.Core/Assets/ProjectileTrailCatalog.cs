using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable small-OBJ appearance keyed by native trail frame; no timing or movement fields.</summary>
public sealed class ProjectileTrailCatalog
{
    private readonly Dictionary<ushort, ushort> suppliedAttributes = [];
    public ProjectileTrailAtlas? Tiles { get; }
    private ProjectileTrailCatalog(Dictionary<ushort, ushort> attributes, ProjectileTrailAtlas? tiles)
    {
        Tiles = tiles;
        for (int index = 0; index < ProjectileTrailVisualDefinitions.Frames.Count; index++)
        {
            ushort frame = ProjectileTrailVisualDefinitions.Frames[index];
            if (attributes[frame] != DefaultAttributes(index)) suppliedAttributes.Add(frame, attributes[frame]);
        }
    }

    /// <summary>$90:B4CB/B52D uses consecutive ice glyphs beginning at tile $38, palette6, priority2.</summary>
    private const int IceFirstTile = 0x38;
    /// <summary>$90:B58F uses four consecutive wave glyphs beginning at tile $3C, palette5, priority2.</summary>
    private const int WaveFirstTile = 0x3c;
    /// <summary>$90:B5A1 uses four consecutive missile glyphs beginning at tile $48, palette5, priority2.</summary>
    private const int MissileFirstTile = 0x48;

    /// <summary>
    /// Shared tile progression and OBJ fields. Ice's independently chosen pose boundaries
    /// at records4,8,16 remain pending source payload; no timing derivation is claimed.
    /// </summary>
    private static ushort DefaultAttributes(int index)
    {
        bool ice = index < 34;
        int phase = ice ? index % 17 : (index - 34) % 4;
        if (ice) phase = phase < 4 ? 0 : phase < 8 ? 1 : phase < 16 ? 2 : 3;
        int firstTile = ice ? IceFirstTile : index < 38 ? WaveFirstTile : MissileFirstTile;
        return SnesObjAttributeWord.Create(firstTile + phase, ice ? 6 : 5, 2, SnesTileFlipFlags.None).Raw;
    }
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    public ushort Resolve(ushort frame)
    {
        if (suppliedAttributes.TryGetValue(frame, out ushort value)) return value;
        for (int index = 0; index < ProjectileTrailVisualDefinitions.Frames.Count; index++)
            if (ProjectileTrailVisualDefinitions.Frames[index] == frame) return DefaultAttributes(index);
        throw new InvalidDataException($"Missing trail frame {frame:X4}.");
    }

    public ushort ResolveCurrent(ushort nextInstruction, ushort nativeAttributes)
    {
        // A frozen, newly allocated stream has not consumed its first record. Preserve
        // the native retained attributes rather than displaying an animation frame early.
        if (nextInstruction is Game.ProjectileTrailDefinitions.Empty or Game.ProjectileTrailDefinitions.LeftIce or
            Game.ProjectileTrailDefinitions.RightIce or Game.ProjectileTrailDefinitions.Wave or Game.ProjectileTrailDefinitions.Missile)
            return nativeAttributes;
        return Resolve(unchecked((ushort)(nextInstruction - 4)));
    }

    public static ProjectileTrailCatalog Load(Stream json, ProjectileTrailAtlas? tiles = null)
    {
        ProjectileTrailDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            ValidateUnique(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ProjectileTrailDocument>(Options)
                ?? throw new InvalidDataException("Trail document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid trail JSON.", error); }
        if (document.Version != ProjectileTrailVisualDefinitions.Version || document.Frames is null ||
            document.Frames.Count != ProjectileTrailVisualDefinitions.Frames.Length)
            throw new InvalidDataException("Trail document requires every native frame and version 1.");
        var attributes = new Dictionary<ushort, ushort>();
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
        {
            if (!document.Frames.TryGetValue(ProjectileTrailVisualDefinitions.Name(frame), out var p) || p is null ||
                (uint)p.TileColumn >= ProjectileSpriteDefinitions.TileColumns || (uint)p.TileRow >= ProjectileSpriteDefinitions.TileRows ||
                (uint)p.Palette > 7 || (uint)p.Priority > 3)
                throw new InvalidDataException($"Missing or invalid trail appearance {frame:X4}.");
            attributes.Add(frame, SnesObjAttributeWord.Create(p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                p.Palette, p.Priority, (p.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (p.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw);
        }
        return new(attributes, tiles);
    }
    public static byte[] Write(ProjectileTrailDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }
    private static void ValidateUnique(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) return;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate trail JSON property.");
            ValidateUnique(property.Value);
        }
    }
}

public sealed record ProjectileTrailDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, ProjectileTrailAppearance> Frames { get; init; }
}
public sealed record ProjectileTrailAppearance
{
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required int Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
