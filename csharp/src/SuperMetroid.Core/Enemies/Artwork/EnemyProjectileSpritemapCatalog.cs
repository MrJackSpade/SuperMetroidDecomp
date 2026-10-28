using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Named, editable bank-$8D enemy-projectile OAM compositions.</summary>
public sealed class EnemyProjectileSpritemapCatalog
{
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

    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyProjectileSpritemaps.ContentIdentity(this);

    /// <summary>Installed ordered OAM parts of each composition frame.</summary>
    internal readonly Dictionary<EnemyProjectileFrameId, SpritePart[]> frames;
    /// <summary>Installed ordered OAM parts of each timed program frame.</summary>
    internal readonly Dictionary<EnemyProjectileFrameId, SpritePart[]> programFrames;

    /// <summary>Retains independently validated compositions and timed-program visual bindings.</summary>
    internal EnemyProjectileSpritemapCatalog(Dictionary<EnemyProjectileFrameId, SpritePart[]> frames,
        Dictionary<EnemyProjectileFrameId, SpritePart[]> programFrames) =>
        (this.frames, this.programFrames) = (frames, programFrames);

    /// <summary>Gets ordered compiled OAM parts by bank-$8D spritemap identity; the native $8000 zero-entry map returns no parts, while an uninstalled nonblank identity is rejected.</summary>
    public ReadOnlyMemory<SpritePart> Get(EnemyProjectileFrameId frame) => EnemyProjectileSpritemaps.Get(this, frame);

    /// <summary>Gets the ordered OAM parts of installed timed program frame <paramref name="frame"/>; an uninstalled frame is rejected.</summary>
    public ReadOnlyMemory<SpritePart> GetProgramFrame(EnemyProjectileFrameId frame) => EnemyProjectileSpritemaps.GetProgramFrame(this, frame);

    /// <summary>Loads named projectile compositions and timed-program visual frames, validating version-specific names/counts and OAM bounds without importing timing, motion, collision, or callbacks.</summary>
    /// <param name="json">Caller-owned <c>enemy-projectile-compositions.json</c> stream, read from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <param name="stock">Current verified installation used to retain later-added frames when loading an older override; without this fallback only the current schema is accepted.</param>
    /// <returns>Compiled packed spritemap parts, preserving supplied part order and merging legacy edits by frame identity.</returns>
    public static EnemyProjectileSpritemapCatalog Load(Stream json,
        EnemyProjectileSpritemapCatalog? stock = null) => EnemyProjectileSpritemaps.Load(json, stock);

    /// <summary>Writes camel-case JSON only after round-trip loading validates the current schema and OAM bounds.</summary>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document) => EnemyProjectileSpritemaps.Write(document);

    /// <summary>Strict editable composition JSON policy, rejecting unknown fields while retaining readable output.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
