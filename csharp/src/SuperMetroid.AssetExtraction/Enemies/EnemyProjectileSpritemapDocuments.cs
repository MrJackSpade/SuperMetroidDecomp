using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemy-projectile composition document (#1463): loading, validating and writing the named bank-$8D compositions
/// and timed program frames the projectile sheets are composed from. Gameplay draws the composed sheets and never reads
/// this document.
/// </summary>
public static class EnemyProjectileSpritemapDocuments
{
    /// <summary>The document schema revision extraction writes and loading accepts.</summary>
    public const int Version = 16;
    /// <summary>Maximum ordered sprite parts admitted per composition; empty compositions are valid.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen 8-pixel character columns per atlas row; row times 16 plus column forms the packed character index.</summary>
    public const int TileColumns = 16;
    /// <summary>Schema-level bound of 64 atlas rows; the selected row/column character number must also fit the sheet during packed-word compilation.</summary>
    public const int TileRows = 64;

    /// <summary>Strict editable composition JSON policy, rejecting unknown fields while retaining readable output.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads named projectile compositions and timed-program visual frames, validating names/counts and part bounds without importing timing, motion, collision, or callbacks.</summary>
    /// <param name="json">Caller-owned <c>enemy-projectile-compositions.json</c> stream, read from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled packed spritemap parts, preserving supplied part order.</returns>
    /// <exception cref="InvalidDataException">The document has another version, the wrong frames, or invalid parts.</exception>
    public static EnemyProjectileSpritemapCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        EnemyProjectileSpritemapDocument document =
            JsonAssetDocument.Read<EnemyProjectileSpritemapDocument>(json, Options, "enemy-projectile compositions");
        if (document.Version != Version || document.Frames is null ||
            document.Frames.Count != EnemyProjectileSpritemapDefinitions.FrameNames.Length)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        var compiled = new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        foreach (string name in EnemyProjectileSpritemapDefinitions.FrameNames)
        {
            if (!document.Frames.TryGetValue(name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing.");
            compiled[EnemyProjectileFrames.Identity(name)] = CompileParts(name, visual);
        }
        ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> definitions = EnemyProjectilePresentationFrameSets.All;
        if (document.ProgramFrames is null ||
            document.ProgramFrames.Count != definitions.Length)
            throw new InvalidDataException(
                "Enemy-projectile program frames have the wrong count.");
        var compiledPrograms = new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        foreach (EnemyProjectilePresentationFrameDefinition frame in definitions)
        {
            if (!document.ProgramFrames.TryGetValue(frame.Name,
                    out SpriteVisualPart[]? visual) || visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile frame {frame.Name} is missing.");
            compiledPrograms[frame.Frame] = CompileParts(frame.Name, visual);
        }
        return new EnemyProjectileSpritemapCatalog(compiled, compiledPrograms);
    }

    /// <summary>Writes the document as camel-case JSON.</summary>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>Validates ordered editable sprite parts against the sheet's bounds.</summary>
    private static SpritePart[] CompileParts(string name, SpriteVisualPart[] visual)
    {
        if (visual.Length > MaximumParts)
            throw new InvalidDataException(
                $"Enemy-projectile composition {name} is oversized.");
        var parts = new SpritePart[visual.Length];
        for (int index = 0; index < visual.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= TileColumns ||
                part.TileRow is < 0 or >= TileRows)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} part {index} is invalid.");
            TileFlip flips =
                (part.FlipX ? TileFlip.Horizontal : 0) |
                (part.FlipY ? TileFlip.Vertical : 0);
            parts[index] = SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16,
                part.TileRow * TileColumns +
                        part.TileColumn, part.Palette.Value, part.Priority, flips);
        }
        return parts;
    }
}
