using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The body canvases bosses assemble besides the shown one (#1395, #1466): Crocomire's death assembles his melt in one,
/// and Mother Brain stages her body in one until her second phase shows it.
/// </summary>
/// <param name="crocomire">Crocomire, whose death sequence assembles his melt.</param>
/// <param name="motherBrain">Mother Brain's encounter, whose body is staged before it is shown.</param>
internal sealed class BossBodyCopies(CrocomireBehavior crocomire, MotherBrainEncounter motherBrain) : IBossBodyCopies
{
    /// <inheritdoc/>
    public bool StagesBody => motherBrain.MotherBrain is not null;

    /// <inheritdoc/>
    public void Draw(BossBodyFrame frame)
    {
        crocomire.Access.CrocomireDeath?.WorkingBody?.Draw(frame);
        motherBrain.MotherBrain?.StagedBody?.Draw(frame);
    }
}
