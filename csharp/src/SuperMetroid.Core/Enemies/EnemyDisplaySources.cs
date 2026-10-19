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

/// <summary>The display sources over the owning species.</summary>
/// <param name="motherBrain">Mother Brain's encounter.</param>
/// <param name="draygon">Draygon.</param>
/// <param name="phantoon">Phantoon.</param>
/// <param name="ridley">Ridley's encounter.</param>
/// <param name="kraid">Kraid.</param>
/// <param name="crocomire">Crocomire.</param>
/// <param name="morphBallEye">The morph-ball eye.</param>
internal sealed class EnemyDisplaySources(
    MotherBrainEncounter motherBrain,
    DraygonBehavior draygon,
    PhantoonBehavior phantoon,
    RidleyEncounter ridley,
    KraidBehavior kraid,
    CrocomireBehavior crocomire,
    MorphBallEyeBehavior morphBallEye) : IEnemyDisplaySources
{
    /// <inheritdoc/>
    public MotherBrainEnemyState? MotherBrain => motherBrain.MotherBrain;

    /// <inheritdoc/>
    public DraygonEnemyState? Draygon => draygon.Draygon;

    /// <inheritdoc/>
    public PhantoonEnemyState? Phantoon => phantoon.Phantoon;

    /// <inheritdoc/>
    public RidleyEnemyState? CeresRidley => ridley.CeresRidley;

    /// <inheritdoc/>
    public BossBg2Layer? KraidBody => kraid.CaptureBodyLayer();

    /// <inheritdoc/>
    public Position16<Bg2Pixels>? CrocomireBg2Scroll => crocomire.Crocomire is not null ? crocomire.CrocomireBg2Scroll : null;

    /// <inheritdoc/>
    public MorphBallEyeBeamState MorphBallEyeBeam => morphBallEye.MorphBallEyeBeam;

    /// <inheritdoc/>
    public IReadOnlyList<MorphBallEyeEnemyState?> MorphBallEyeStates => morphBallEye.MorphBallEyeStates;
}
