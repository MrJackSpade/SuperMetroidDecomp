using SuperMetroid.Core.Spatial;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Immutable single-piece appearance keyed by native trail frame; no timing or movement fields.</summary>
public sealed class InstalledProjectileTrails
{
    /// <summary>Only authored appearances differing from the shared native tile progressions.</summary>
    internal readonly Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, SpritePart> suppliedAttributes = [];
    /// <summary>Copies the frame appearances that differ from the shared native tile progressions.</summary>
    private InstalledProjectileTrails(Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, SpritePart> attributes)
    {
        for (int index = 0; index < ProjectileTrailVisualDefinitions.FrameCount; index++)
        {
            var frame = ProjectileTrailVisualDefinitions.Frames[index];
            if (attributes[frame] != DefaultAttributes(index)) suppliedAttributes.Add(frame, attributes[frame]);
        }
    }

    /// <summary>
    /// Shared character progression, palette and priority. Ice's independently chosen pose boundaries
    /// at records4,8,16 remain pending source payload; no timing derivation is claimed.
    /// </summary>
    private static SpritePart DefaultAttributes(int index)
    {
        bool ice = index < 34;
        int phase = ice ? index % 17 : (index - 34) % 4;
        if (ice) phase = phase < 4 ? 0 : phase < 8 ? 1 : phase < 16 ? 2 : 3;
        int firstTile = ice ? ProjectileTrailGlyphDefinitions.IceFirstTile
            : index < 38 ? ProjectileTrailGlyphDefinitions.WaveFirstTile : ProjectileTrailGlyphDefinitions.MissileFirstTile;
        return SpritePart.Single(firstTile + phase, ice ? 6 : 5, 2);
    }
    /// <summary>Uses stable camel-case output and rejects unknown appearance properties.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    /// <summary>Resolves one trail frame to its selected single-piece appearance.</summary>
    /// <param name="frame">The trail frame, among the 42 left/right ice, wave and missile poses.</param>
    /// <returns>The character, palette, priority and mirroring.</returns>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a trail frame.</exception>
    public SpritePart Resolve(SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame frame)
    {
        if (suppliedAttributes.ContainsKey(frame)) return suppliedAttributes[frame];
        for (int index = 0; index < ProjectileTrailVisualDefinitions.FrameCount; index++)
            if (ProjectileTrailVisualDefinitions.Frames[index] == frame) return DefaultAttributes(index);
        throw new InvalidDataException($"Missing trail frame {frame}.");
    }

    /// <summary>Validates every native trail-frame appearance, retaining only differences from the default appearances.</summary>
    /// <param name="json">Case-sensitive UTF-8 JSON read from its current position; the caller retains ownership of the stream.</param>
    /// <returns>An immutable appearance catalog that does not retain the document's mutable frame dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate/unknown properties, lacks the exact 42 native frame keys or version 1, or has invalid tile, palette or priority values.</exception>
    public static InstalledProjectileTrails Load(Stream json)
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
        var attributes = new Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, SpritePart>();
        for (int index = 0; index < ProjectileTrailVisualDefinitions.FrameCount; index++)
        {
            var frame = ProjectileTrailVisualDefinitions.Frames[index];
            string name = ProjectileTrailVisualDefinitions.FrameNames[index];
            if (!document.Frames.TryGetValue(name, out var p) || p is null ||
                (uint)p.TileColumn >= ProjectileSpriteDefinitions.TileColumns || (uint)p.TileRow >= ProjectileSpriteDefinitions.TileRows ||
                (uint)p.Palette > 7 || (uint)p.Priority > 3)
                throw new InvalidDataException($"Missing or invalid trail appearance {name}.");
            attributes.Add(frame, SpritePart.Single(p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                p.Palette, p.Priority, (p.FlipX ? TileFlip.Horizontal : 0) | (p.FlipY ? TileFlip.Vertical : 0)));
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
