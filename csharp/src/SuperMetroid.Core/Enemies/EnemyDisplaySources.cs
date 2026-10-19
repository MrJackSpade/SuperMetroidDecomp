using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Enemies;

/// <summary>The display sources over the owning species of a room's enemies.</summary>
/// <param name="enemies">The room's enemies: their boss encounters, the morph-ball eye and their presentation.</param>
internal sealed class EnemyDisplaySources(RoomEnemySystem enemies) : IEnemyDisplaySources
{
    /// <inheritdoc/>
    public MotherBrainEnemyState? MotherBrain => enemies.MotherBrainEncounter.MotherBrain;

    /// <inheritdoc/>
    public DraygonEnemyState? Draygon => enemies.DraygonBehavior.Draygon;

    /// <inheritdoc/>
    public PhantoonEnemyState? Phantoon => enemies.PhantoonBehavior.Phantoon;

    /// <inheritdoc/>
    public RidleyEnemyState? CeresRidley => enemies.RidleyEncounter.CeresRidley;

    /// <inheritdoc/>
    public BossBg2Layer? KraidBody => enemies.KraidBehavior.CaptureBodyLayer();

    /// <inheritdoc/>
    public EnemyBg2ImageSnapshot? EnemyBg2 => enemies.Presentation.Bg2Image?.Publish();

    /// <inheritdoc/>
    public Position16<Bg2Pixels>? CrocomireBg2Scroll => enemies.CrocomireBehavior.Crocomire is not null ? enemies.CrocomireBehavior.CrocomireBg2Scroll : null;

    /// <inheritdoc/>
    public MorphBallEyeBeamState MorphBallEyeBeam => enemies.MorphBallEyeBehavior.MorphBallEyeBeam;

    /// <inheritdoc/>
    public IReadOnlyList<MorphBallEyeEnemyState?> MorphBallEyeStates => enemies.MorphBallEyeBehavior.MorphBallEyeStates;
}
