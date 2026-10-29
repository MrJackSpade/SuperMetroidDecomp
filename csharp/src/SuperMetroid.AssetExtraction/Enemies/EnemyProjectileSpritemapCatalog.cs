using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Named, editable bank-$8D enemy-projectile OAM compositions, which the import pipeline composes the spawned-object
/// sheets' projectile pictures from (#1463).
/// </summary>
public sealed class EnemyProjectileSpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyProjectileSpritemaps.ContentIdentity(this);

    /// <summary>The installed compositions and timed program frames.</summary>
    internal readonly EnemyProjectileSpritemapSelection selection;

    /// <summary>Retains independently validated compositions and timed-program visual bindings.</summary>
    internal EnemyProjectileSpritemapCatalog(Dictionary<EnemyProjectileFrameId, SpritePart[]> frames,
        Dictionary<EnemyProjectileFrameId, SpritePart[]> programFrames) =>
        selection = new(frames, programFrames);

    /// <summary>Gets ordered compiled OAM parts by bank-$8D spritemap identity; the native $8000 zero-entry map returns no parts, while an uninstalled nonblank identity is rejected.</summary>
    public ReadOnlyMemory<SpritePart> Get(EnemyProjectileFrameId frame) => EnemyProjectileSpritemaps.Get(this, frame);

    /// <summary>Gets the ordered OAM parts of installed timed program frame <paramref name="frame"/>; an uninstalled frame is rejected.</summary>
    public ReadOnlyMemory<SpritePart> GetProgramFrame(EnemyProjectileFrameId frame) => EnemyProjectileSpritemaps.GetProgramFrame(this, frame);
}
