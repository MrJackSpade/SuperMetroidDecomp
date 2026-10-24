using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable small-OBJ appearance keyed by native trail frame; no timing or movement fields.</summary>
public sealed class ProjectileTrailCatalog
{
    /// <summary>Only authored OBJ attribute words differing from the shared native tile progressions.</summary>
    internal readonly Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, ushort> suppliedAttributes = [];
    /// <summary>Copies the frame appearances that differ from the shared native tile progressions.</summary>
    private ProjectileTrailCatalog(Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, ushort> attributes)
    {
        for (int index = 0; index < ProjectileTrailVisualDefinitions.FrameCount; index++)
        {
            var frame = ProjectileTrailVisualDefinitions.Frames[index];
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
    /// <summary>Uses stable camel-case output and rejects unknown appearance properties.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    /// <summary>Resolves one trail frame to its selected small-OBJ tile and attribute word.</summary>
    /// <param name="frame">The trail frame, among the 42 left/right ice, wave and missile poses.</param>
    /// <returns>The complete nine-bit character number, palette, priority and reflection flags packed in native OBJ attribute form.</returns>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a trail frame.</exception>
    public ushort Resolve(SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame frame)
    {
        if (suppliedAttributes.TryGetValue(frame, out ushort value)) return value;
        for (int index = 0; index < ProjectileTrailVisualDefinitions.FrameCount; index++)
            if (ProjectileTrailVisualDefinitions.Frames[index] == frame) return DefaultAttributes(index);
        throw new InvalidDataException($"Missing trail frame {frame}.");
    }

    /// <summary>Validates every native trail-frame appearance and compiles independent OBJ attributes, retaining only differences from the default appearances.</summary>
    /// <param name="json">Case-sensitive UTF-8 JSON read from its current position; the caller retains ownership of the stream.</param>
    /// <returns>An immutable appearance catalog that does not retain the document's mutable frame dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate/unknown properties, lacks the exact 42 native frame keys or version 1, or has invalid tile, palette or priority values.</exception>
    public static ProjectileTrailCatalog Load(Stream json)
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
            document.Frames.Count != ProjectileTrailVisualDefinitions.FrameCount)
            throw new InvalidDataException("Trail document requires every native frame and version 1.");
        var attributes = new Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, ushort>();
        for (int index = 0; index < ProjectileTrailVisualDefinitions.FrameCount; index++)
        {
            var frame = ProjectileTrailVisualDefinitions.Frames[index];
            string name = ProjectileTrailVisualDefinitions.FrameNames[index];
            if (!document.Frames.TryGetValue(name, out var p) || p is null ||
                (uint)p.TileColumn >= ProjectileSpriteDefinitions.TileColumns || (uint)p.TileRow >= ProjectileSpriteDefinitions.TileRows ||
                (uint)p.Palette > 7 || (uint)p.Priority > 3)
                throw new InvalidDataException($"Missing or invalid trail appearance {name}.");
            attributes.Add(frame, SnesObjAttributeWord.Create(p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                p.Palette, p.Priority, (p.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (p.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw);
        }
        return new(attributes);
    }
    /// <summary>Serializes and validates all trail appearances using the same strict schema as <see cref="Load"/>.</summary>
    /// <param name="document">Authored frame dictionary read for serialization, not retained by the writer.</param>
    /// <returns>A new caller-owned UTF-8 JSON byte array with camel-case properties and indented formatting.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails native-frame completeness or appearance validation.</exception>
    public static byte[] Write(ProjectileTrailDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }
    /// <summary>Recursively rejects repeated object keys before deserialization could replace a trail frame.</summary>
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
