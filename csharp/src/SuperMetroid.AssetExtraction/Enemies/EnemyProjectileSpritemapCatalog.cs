using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Named, editable bank-$8D enemy-projectile OAM compositions, which the import pipeline composes the spawned-object
/// sheets' projectile pictures from (#1463).
/// </summary>
public sealed partial class EnemyProjectileSpritemapCatalog
{

    /// <summary>The installed compositions and timed program frames.</summary>
    internal readonly EnemyProjectileSpritemapSelection selection;

    /// <summary>Retains independently validated compositions, timed-program visual bindings and the Ceres elevator arrival's pictures.</summary>
    internal EnemyProjectileSpritemapCatalog(Dictionary<EnemyProjectileFrameId, SpritePart[]> frames,
        Dictionary<EnemyProjectileFrameId, SpritePart[]> programFrames, Dictionary<CeresElevatorArrivalPose, SpritePart[]> elevatorArrival) =>
        selection = new(frames, programFrames, elevatorArrival);

    /// <summary>Gets ordered compiled OAM parts by bank-$8D spritemap identity; the native $8000 zero-entry map returns no parts, while an uninstalled nonblank identity is rejected.</summary>
    public ReadOnlyMemory<SpritePart> Get(EnemyProjectileFrameId frame) => EnemyProjectileSpritemaps.Get(this, frame);

    /// <summary>Gets the ordered OAM parts of installed timed program frame <paramref name="frame"/>; an uninstalled frame is rejected.</summary>
    public ReadOnlyMemory<SpritePart> GetProgramFrame(EnemyProjectileFrameId frame) => EnemyProjectileSpritemaps.GetProgramFrame(this, frame);

    /// <summary>Gets the ordered OAM parts of the Ceres elevator arrival's picture of <paramref name="pose"/> (#2425).</summary>
    internal ReadOnlyMemory<SpritePart> GetElevatorArrival(CeresElevatorArrivalPose pose) => Ensure.Listed(selection.ElevatorArrival, pose);
}
