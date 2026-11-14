using SuperMetroid.Core.Spatial;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Immutable single-piece appearance keyed by native trail frame; no timing or movement fields.</summary>
public sealed class InstalledProjectileTrails
{
    /// <summary>The installed appearance of every trail frame (#1596).</summary>
    private readonly Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, SpritePart> attributes;
    /// <summary>Takes the validated appearance of every trail frame.</summary>
    private InstalledProjectileTrails(Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, SpritePart> attributes) =>
        this.attributes = attributes;

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
    public SpritePart Resolve(SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame frame) =>
        attributes.TryGetValue(frame, out SpritePart appearance) ? appearance : throw new InvalidDataException($"Missing trail frame {frame}.");

    /// <summary>Validates every native trail-frame appearance and keeps each one as installed.</summary>
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
                p.Palette, p.Priority, (p.FlipX ? PictureFlip.Horizontal : 0) | (p.FlipY ? PictureFlip.Vertical : 0)));
        }
        return new(attributes);
    }
    /// <summary>Serializes all trail appearances in the schema <see cref="Load"/> reads.</summary>
    /// <param name="document">Authored frame dictionary read for serialization, not retained by the writer.</param>
    /// <returns>A new caller-owned UTF-8 JSON byte array with camel-case properties and indented formatting.</returns>
    public static byte[] Write(ProjectileTrailDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);
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
