using SuperMetroid.Core.Spatial;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Immutable, ROM-independent projectile visual compositions; no damage, collision or timing fields.</summary>
public sealed class InstalledProjectileSprites
{
    /// <summary>Compiled OAM compositions keyed by composition.</summary>
    internal readonly Dictionary<ProjectileSpriteId, SpriteComposition> frames;
    /// <summary>Accepts the complete validated set of compositions.</summary>
    private InstalledProjectileSprites(Dictionary<ProjectileSpriteId, SpriteComposition> frames) => this.frames = frames;

    /// <summary>One installed projectile composition as a whole frame.</summary>
    /// <param name="id">The composition.</param>
    /// <returns>The composition as a whole frame whose characters are its OBJ tile numbers.</returns>
    public SpriteFrame Frame(ProjectileSpriteId id)
    {
        if (!frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id.Name}.");
        return wholeFrames.GetOrAdd(id, key => parts.ToFrame($"Projectile.{key.Name}", 0));
    }

    /// <summary>Whole frames already built, by composition.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ProjectileSpriteId, SpriteFrame> wholeFrames = new();

    /// <summary>Validates and compiles the complete required projectile spritemap set into independently owned visual compositions, including the empty Nothing frame.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Installed artwork keyed by the 417 compositions timed records draw, each exactly as installed (#1596).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or ambiguous, its schema or required identity set is invalid, or a part exceeds the supported OAM fields or count.</exception>
    public static InstalledProjectileSprites Load(Stream json)
        => LoadFrames(json, ProjectileSpriteDefinitions.All);

    /// <summary>
    /// Loads the native part format for either timed projectiles or charge flares. Charge
    /// flares have a separate required composition set.
    /// </summary>
    internal static InstalledProjectileSprites LoadFrames(Stream json, IReadOnlyList<ProjectileSpriteId> required)
    {
        int requiredCount = required.Count;
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
        if (document.Version != ProjectileSpriteDefinitions.Version || document.Frames is null || document.Frames.Count != requiredCount)
            throw new InvalidDataException($"Projectile compositions require version {ProjectileSpriteDefinitions.Version} and every required composition.");
        var frames = new Dictionary<ProjectileSpriteId, SpriteComposition>();
        foreach (ProjectileSpriteId id in required)
        {
            string name = id.Name;
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
                compiled[i] = new(SpritePart.Create(Offset16<ScreenPixels>.Of(p.OffsetX, p.OffsetY), p.Size == 16,
                    p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                    p.Palette.Value, p.Priority, (p.FlipX ? PictureFlip.Horizontal : 0) | (p.FlipY ? PictureFlip.Vertical : 0)), false);
            }
            frames.Add(id, new SpriteComposition(compiled));
        }
        return new(frames);
    }

    /// <summary>Rejects ambiguous JSON fields before compiling the required sprite set.</summary>
    private static void RejectDuplicateProperties(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate projectile composition property {name}."));
}
