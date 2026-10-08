using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable small-OBJ appearance keyed by native trail frame; no timing or movement fields.</summary>
public sealed class ProjectileTrailCatalog
{
    private readonly Dictionary<ushort, ushort> suppliedAttributes = [];
    /// <summary>Optional installed twelve-tile trail artwork provider retained from loading; resolving appearances neither uploads nor replaces its pixels.</summary>
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
    /// <summary>Resolves one native timed trail record to its selected small-OBJ tile and attribute word without advancing its instruction stream.</summary>
    /// <param name="frame">Bank-$90 address of the timed record's duration word, among the 42 appearance-bearing records in the left/right ice, wave and missile lists.</param>
    /// <returns>The complete nine-bit character number, palette, priority and reflection flags packed in native OBJ attribute form.</returns>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not an authored timed-record identity.</exception>
    public ushort Resolve(ushort frame)
    {
        if (suppliedAttributes.TryGetValue(frame, out ushort value)) return value;
        for (int index = 0; index < ProjectileTrailVisualDefinitions.Frames.Count; index++)
            if (ProjectileTrailVisualDefinitions.Frames[index] == frame) return DefaultAttributes(index);
        throw new InvalidDataException($"Missing trail frame {frame:X4}.");
    }

    /// <summary>Resolves the last consumed trail appearance from the retained instruction cursor, preserving attributes when a newly allocated stream has not consumed a record.</summary>
    /// <param name="nextInstruction">Bank-$90 next-record cursor; after a timed record it points four bytes beyond that record.</param>
    /// <param name="nativeAttributes">Retained tile/attribute word returned unchanged for Empty or a left-ice, right-ice, wave or missile list-start cursor, including while time is frozen.</param>
    /// <returns>The selected appearance for the previous timed record, or the unchanged retained word before the first record is consumed.</returns>
    /// <exception cref="InvalidDataException">A noninitial cursor minus four does not identify an authored timed record.</exception>
    public ushort ResolveCurrent(ushort nextInstruction, ushort nativeAttributes)
    {
        // A frozen, newly allocated stream has not consumed its first record. Preserve
        // the native retained attributes rather than displaying an animation frame early.
        if (nextInstruction is Game.ProjectileTrailDefinitions.Empty or Game.ProjectileTrailDefinitions.LeftIce or
            Game.ProjectileTrailDefinitions.RightIce or Game.ProjectileTrailDefinitions.Wave or Game.ProjectileTrailDefinitions.Missile)
            return nativeAttributes;
        return Resolve(unchecked((ushort)(nextInstruction - 4)));
    }

    /// <summary>Validates every native trail-frame appearance and compiles independent OBJ attributes, retaining only differences from the default appearances.</summary>
    /// <param name="json">Case-sensitive UTF-8 JSON read from its current position; the caller retains ownership of the stream.</param>
    /// <param name="tiles">Optional tile provider associated with this catalog by reference, without uploading graphics or changing frame validation.</param>
    /// <returns>An immutable appearance catalog that does not retain the document's mutable frame dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate/unknown properties, lacks the exact 42 native frame keys or version 1, or has invalid tile, palette or priority values.</exception>
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

/// <summary>Editable appearance-only schema for trail timed records; its frame dictionary remains caller-mutable and contains no movement commands or frame durations.</summary>
public sealed record ProjectileTrailDocument
{
    /// <summary>Schema revision; loading requires <see cref="ProjectileTrailVisualDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 42 nonnull appearances keyed as trail_XXXX with uppercase hexadecimal bank-$90 timed-record addresses; left/right ice each contribute 17, and wave/missile each contribute four.</summary>
    public required Dictionary<string, ProjectileTrailAppearance> Frames { get; init; }
}
/// <summary>One small eight-by-eight OBJ's selected character and attributes, using the full gameplay projectile tile layout rather than coordinates in the compact trail PNG.</summary>
public sealed record ProjectileTrailAppearance
{
    /// <summary>Zero-based gameplay OBJ tile column 0..15; with the row this produces character number row * 16 + column.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based gameplay OBJ tile row 0..31, spanning all 512 character numbers including the tile-page bit.</summary>
    public required int TileRow { get; init; }
    /// <summary>OBJ palette selector 0..7 encoded in attribute bits 9..11; stock ice uses 6 and wave/missile trails use 5.</summary>
    public required int Palette { get; init; }
    /// <summary>OBJ priority tier 0..3 encoded in bits 12..13; stock trail records use tier 2.</summary>
    public required int Priority { get; init; }
    /// <summary>Whether the selected eight-pixel tile is reflected horizontally via OBJ attribute bit 14.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether the selected eight-pixel tile is reflected vertically via OBJ attribute bit 15.</summary>
    public required bool FlipY { get; init; }
}
