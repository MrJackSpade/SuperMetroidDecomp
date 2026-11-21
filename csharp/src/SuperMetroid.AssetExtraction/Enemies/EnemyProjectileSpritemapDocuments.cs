using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Areas.Ceres;
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
    /// <summary>
    /// The document schema revision extraction writes and loading accepts; 17 keys every program frame by what its picture
    /// shows (#2154), and 18 names each part's character picture by its place among the projectile characters instead of an
    /// atlas row and column (#2563).
    /// </summary>
    public const int Version = 18;
    /// <summary>Maximum ordered sprite parts admitted per composition; empty compositions are valid.</summary>
    public const int MaximumParts = 128;
    /// <summary>The character pictures the projectile characters hold, one of which each part names.</summary>
    public const int Characters = 1024;

    /// <summary>Strict editable composition JSON policy, rejecting unknown fields while retaining readable output.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads named projectile compositions and timed-program visual frames, validating names/counts and part bounds without importing timing, motion, collision, or callbacks.</summary>
    /// <param name="json">Caller-owned <c>enemy-projectile-compositions.json</c> stream, read from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>The compositions' sprite parts, preserving supplied part order.</returns>
    /// <exception cref="InvalidDataException">The document has another version, the wrong frames, or invalid parts.</exception>
    public static EnemyProjectileSpritemapCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        EnemyProjectileSpritemapDocument document =
            JsonAssetDocument.Read<EnemyProjectileSpritemapDocument>(json, Options, "enemy-projectile compositions");
        if (document.Version != Version || document.Frames is null ||
            document.Frames.Count != EnemyProjectileSpritemapDefinitions.DocumentFrameNames.Count)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        var compiled = new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        foreach (string name in EnemyProjectileSpritemapDefinitions.FrameNames)
        {
            if (!document.Frames.TryGetValue(name, out EnemyProjectilePartDocument[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing.");
            compiled[EnemyProjectileFrames.Identity(name)] = CompileParts(name, visual);
        }
        // The Ceres elevator arrival's pictures are composed as the projectiles' are, but show its own poses (#2425).
        var arrival = new Dictionary<CeresElevatorArrivalPose, SpritePart[]>();
        foreach ((CeresElevatorArrivalPose pose, string name) in EnemyProjectileSpritemapDefinitions.ElevatorArrivalFrameNames)
        {
            if (!document.Frames.TryGetValue(name, out EnemyProjectilePartDocument[]? visual) || visual is null)
                throw new InvalidDataException($"Enemy-projectile composition {name} is missing.");
            arrival[pose] = CompileParts(name, visual);
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
                    out EnemyProjectilePartDocument[]? visual) || visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile frame {frame.Name} is missing.");
            compiledPrograms[frame.Frame] = CompileParts(frame.Name, visual);
        }
        return new EnemyProjectileSpritemapCatalog(compiled, compiledPrograms, arrival);
    }

    /// <summary>Writes the document as camel-case JSON.</summary>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Enemies.EnemyProjectileSpritemapDocument>(Options));

    /// <summary>
    /// Validates the import data's ordered sprite parts against the sheet's bounds and decodes them into the parts the import
    /// composes the pictures from; the installation holds the pictures (#1638).
    /// </summary>
    private static SpritePart[] CompileParts(string name, EnemyProjectilePartDocument[] visual)
    {
        if (visual.Length > MaximumParts)
            throw new InvalidDataException(
                $"Enemy-projectile composition {name} is oversized.");
        var parts = new SpritePart[visual.Length];
        for (int index = 0; index < visual.Length; index++)
        {
            EnemyProjectilePartDocument? part = visual[index];
            if (part is null || part.Size is not (8 or 16) ||
                part.Palette is null or < 0 or > 7 ||
                part.Character is < 0 or >= Characters)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} part {index} is invalid.");
            PictureFlip flips =
                (part.FlipX ? PictureFlip.Horizontal : 0) |
                (part.FlipY ? PictureFlip.Vertical : 0);
            parts[index] = SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16,
                part.Character, part.Palette.Value, part.Depth, flips);
        }
        return parts;
    }
}
