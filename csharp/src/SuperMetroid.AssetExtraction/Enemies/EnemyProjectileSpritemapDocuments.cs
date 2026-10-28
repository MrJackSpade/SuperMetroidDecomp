using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemy-projectile composition document (#1463): loading, validating and writing the named bank-$8D compositions
/// and timed program frames the projectile sheets are composed from. Gameplay draws the composed sheets and never reads
/// this document.
/// </summary>
public static class EnemyProjectileSpritemapDocuments
{
    /// <summary>Three bank-$8D compositions in the version-1 Ceres-only catalog, retained when merging that legacy override with current stock.</summary>
    public const int LegacyFrameCount = 3;
    /// <summary>Maximum ordered sprite parts admitted per composition; empty compositions are valid.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen 8-pixel character columns per atlas row; row times 16 plus column forms the packed character index.</summary>
    public const int TileColumns = 16;
    /// <summary>Schema-level bound of 64 atlas rows; the selected row/column character number must also fit the sheet during packed-word compilation.</summary>
    public const int TileRows = 64;

    /// <summary>The schema generation each document version number names.</summary>
    internal static readonly ParameterVariants<EnemyProjectileSpritemapVersion> Versions = new(new Dictionary<int, EnemyProjectileSpritemapVersion>
    {
        [1] = EnemyProjectileSpritemapVersion.CeresOnly,
        [2] = EnemyProjectileSpritemapVersion.PreProgramFrames,
        [3] = EnemyProjectileSpritemapVersion.FirstProgramFrame,
        [4] = EnemyProjectileSpritemapVersion.PreAlcoon,
        [5] = EnemyProjectileSpritemapVersion.PreGoldenTorizo,
        [6] = EnemyProjectileSpritemapVersion.PreGoldenTorizoEgg,
        [7] = EnemyProjectileSpritemapVersion.PreTorizoEffects,
        [8] = EnemyProjectileSpritemapVersion.PreGenericEnemyDeath,
        [9] = EnemyProjectileSpritemapVersion.PreEnvironmentAndAttack,
        [10] = EnemyProjectileSpritemapVersion.PreMotherBrainAndStatue,
        [11] = EnemyProjectileSpritemapVersion.PreWorkRobot,
        [12] = EnemyProjectileSpritemapVersion.PrePolypRock,
        [13] = EnemyProjectileSpritemapVersion.Current,
    });

    /// <summary>Strict editable composition JSON policy, rejecting unknown fields while retaining readable output.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads named projectile compositions and timed-program visual frames, validating version-specific names/counts and part bounds without importing timing, motion, collision, or callbacks.</summary>
    /// <param name="json">Caller-owned <c>enemy-projectile-compositions.json</c> stream, read from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <param name="stock">Current verified installation used to retain later-added frames when loading an older override; without this fallback only the current schema is accepted.</param>
    /// <returns>Compiled packed spritemap parts, preserving supplied part order and merging legacy edits by frame identity.</returns>
    /// <exception cref="InvalidDataException">The document has an unknown version, the wrong frames, or invalid parts.</exception>
    public static EnemyProjectileSpritemapCatalog Load(Stream json,
        EnemyProjectileSpritemapCatalog? stock = null)
    {
        Ensure.NotNull(json);
        EnemyProjectileSpritemapDocument document;
        document = JsonAssetDocument.Read<EnemyProjectileSpritemapDocument>(json, Options, "enemy-projectile compositions");
        if (!Versions.TryDecode(document.Version, out EnemyProjectileSpritemapVersion version))
            throw new InvalidDataException(
                $"Enemy-projectile compositions have unknown schema version {document.Version}.");
        bool legacyOverride = version != EnemyProjectileSpritemapVersion.Current && stock is not null;
        int expectedFrames = version == EnemyProjectileSpritemapVersion.CeresOnly && legacyOverride
            ? LegacyFrameCount
            : EnemyProjectileSpritemapDefinitions.FrameNames.Length;
        if ((!legacyOverride && version != EnemyProjectileSpritemapVersion.Current) ||
            document.Frames is null || document.Frames.Count != expectedFrames)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        // Older overrides retain their edited Ceres/debris frames. Newly introduced
        // shared-program frames come only from the hash-checked stock installation.
        var compiled = stock is not null && legacyOverride
            ? new Dictionary<EnemyProjectileFrameId, SpritePart[]>(stock.selection.Frames)
            : new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        var compiledPrograms = stock is not null && legacyOverride
            ? new Dictionary<EnemyProjectileFrameId, SpritePart[]>(stock.selection.ProgramFrames)
            : new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        foreach (string name in EnemyProjectileSpritemapDefinitions.FrameNames.Take(expectedFrames))
        {
            if (!document.Frames.TryGetValue(name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing.");
            compiled[EnemyProjectileFrames.Identity(name)] = CompileParts(name, visual);
        }
        if (version is not (EnemyProjectileSpritemapVersion.CeresOnly or EnemyProjectileSpritemapVersion.PreProgramFrames))
        {
            EnemyProjectilePresentationFrameDefinition[] definitions = version switch
            {
                EnemyProjectileSpritemapVersion.FirstProgramFrame =>
                    [.. EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames],
                EnemyProjectileSpritemapVersion.PreAlcoon =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizo
                        .WhereToArray(frame => !IsAlcoonFireballFrame(frame.Frame))
                        ,
                EnemyProjectileSpritemapVersion.PreGoldenTorizo =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizo.ToArray(),
                EnemyProjectileSpritemapVersion.PreGoldenTorizoEgg =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizoEgg.ToArray(),
                EnemyProjectileSpritemapVersion.PreTorizoEffects =>
                    EnemyProjectilePresentationFrameSets.PreTorizoEffects.ToArray(),
                EnemyProjectileSpritemapVersion.PreGenericEnemyDeath =>
                    EnemyProjectilePresentationFrameSets.PreGenericEnemyDeath.ToArray(),
                EnemyProjectileSpritemapVersion.PreEnvironmentAndAttack =>
                    EnemyProjectilePresentationFrameSets.PreEnvironmentAndAttack.ToArray(),
                EnemyProjectileSpritemapVersion.PreMotherBrainAndStatue =>
                    EnemyProjectilePresentationFrameSets.PreMotherBrainAndStatue.ToArray(),
                EnemyProjectileSpritemapVersion.PreWorkRobot =>
                    EnemyProjectilePresentationFrameSets.PreWorkRobot.ToArray(),
                EnemyProjectileSpritemapVersion.PrePolypRock =>
                    EnemyProjectilePresentationFrameSets.PrePolypRock.ToArray(),
                EnemyProjectileSpritemapVersion.Current =>
                    EnemyProjectilePresentationFrameSets.All.ToArray(),
                EnemyProjectileSpritemapVersion.CeresOnly or
                    EnemyProjectileSpritemapVersion.PreProgramFrames => throw new InvalidOperationException(
                    $"Enemy-projectile schema {version} has no program frames."),
                _ => throw new InvalidOperationException(
                    $"Enemy-projectile schema {version} has no program frames."),
            };
            if (document.ProgramFrames is null ||
                document.ProgramFrames.Count != definitions.Length)
                throw new InvalidDataException(
                    $"Version-{document.Version} enemy-projectile program frames have the wrong count.");
            foreach (EnemyProjectilePresentationFrameDefinition frame in definitions)
            {
                if (!document.ProgramFrames.TryGetValue(frame.Name,
                        out SpriteVisualPart[]? visual) || visual is null)
                    throw new InvalidDataException(
                        $"Version-{document.Version} enemy-projectile frame {frame.Name} is missing.");
                compiledPrograms[frame.Frame] = CompileParts(frame.Name, visual);
            }
        }
        return new EnemyProjectileSpritemapCatalog(compiled, compiledPrograms);
    }

    /// <summary>Writes camel-case JSON only after round-trip loading validates the current schema and part bounds.</summary>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>Identifies the Alcoon fireball frames the pre-Alcoon legacy schema omitted.</summary>
    private static bool IsAlcoonFireballFrame(EnemyProjectileFrameId frame) =>
        AlcoonFireballFrameDefinitions.ShownProjectileFrames.Values.Contains(frame);

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
