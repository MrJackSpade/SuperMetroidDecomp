using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable small-OBJ appearance keyed by native trail frame; no timing or movement fields.</summary>
public sealed class ProjectileTrailCatalog
{
    private readonly Dictionary<ushort, ushort> attributes;
    public ProjectileTrailAtlas? Tiles { get; }
    private ProjectileTrailCatalog(Dictionary<ushort, ushort> attributes, ProjectileTrailAtlas? tiles)
    { this.attributes = attributes; Tiles = tiles; }
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    public ushort Resolve(ushort frame) => attributes.TryGetValue(frame, out ushort value)
        ? value : throw new InvalidDataException($"Missing trail frame {frame:X4}.");

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
