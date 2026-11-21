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
    /// <param name="frame">The trail frame: one of the ice sparkles', the wave trail's or the missile trail's.</param>
    /// <returns>The character, palette, depth and mirroring.</returns>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a trail frame.</exception>
    public SpritePart Resolve(SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame frame) =>
        attributes.TryGetValue(frame, out SpritePart appearance) ? appearance : throw new InvalidDataException($"Missing trail frame {frame}.");

    /// <summary>Validates every native trail-frame appearance and keeps each one as installed.</summary>
    /// <param name="json">Case-sensitive UTF-8 JSON read from its current position; the caller retains ownership of the stream.</param>
    /// <returns>An immutable appearance catalog that does not retain the document's mutable frame dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">
    /// JSON is malformed, contains duplicate/unknown properties, lacks the exact 42 native record keys or version 1, has
    /// invalid tile or palette values, or gives two records of one frame different appearances (#2348).
    /// </exception>
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
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame[] shown = [.. ProjectileTrailSources.Records.Select(record => record.Value).Distinct()];
        if (document.Version != ProjectileTrailVisualDefinitions.Version || document.Frames is null ||
            document.Frames.Count != shown.Length)
            throw new InvalidDataException($"Trail document requires every trail frame and version {ProjectileTrailVisualDefinitions.Version}.");
        var attributes = new Dictionary<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame, SpritePart>();
        foreach (SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame frame in shown)
        {
            string name = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrameKeys.Installed.Of(frame);
            if (!document.Frames.TryGetValue(name, out var p) || p is null ||
                (uint)p.TileColumn >= ProjectileSpriteDefinitions.TileColumns || (uint)p.TileRow >= ProjectileSpriteDefinitions.TileRows ||
                (uint)p.Palette > 7)
                throw new InvalidDataException($"Missing or invalid trail appearance {name}.");
            SpritePart appearance = SpritePart.Single(p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                p.Palette, p.Depth, (p.FlipX ? PictureFlip.Horizontal : 0) | (p.FlipY ? PictureFlip.Vertical : 0));
            attributes.Add(frame, appearance);
        }
        return new(attributes);
    }
    /// <summary>Serializes all trail appearances in the schema <see cref="Load"/> reads.</summary>
    /// <param name="document">Authored frame dictionary read for serialization, not retained by the writer.</param>
    /// <returns>A new caller-owned UTF-8 JSON byte array with camel-case properties and indented formatting.</returns>
    public static byte[] Write(ProjectileTrailDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);
    /// <summary>Rejects repeated object keys before deserialization could replace a trail frame.</summary>
    private static void ValidateUnique(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            _ => new InvalidDataException("Duplicate trail JSON property."), DuplicatePropertyScope.OutsideArrays);
}
