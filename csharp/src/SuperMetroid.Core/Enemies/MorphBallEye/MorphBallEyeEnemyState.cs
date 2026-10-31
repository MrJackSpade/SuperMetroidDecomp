using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The state of retail enemy <c>$E6BF</c>. The mount and eye body share a definition, but parameter two's sign selects
/// their role. The values the game keeps in the enemy's common record are emptied with it when the enemy is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class MorphBallEyeEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<MorphBallEyeProgram> Program { get; set; }

    /// <summary>
    /// Whether the eye is activated (<c>Eye.activatedFlag</c>): set once the beam object has initialized, cleared once
    /// the body starts closing. The beam keeps cycling its colours only while it is set.
    /// </summary>
    public bool IsActivated { get; internal set; }

    /// <summary>
    /// Zero-up, clockwise, 1/256-turn aim angle.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Activation/deactivation countdown.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }

    /// <summary>Indirect function word; none once a kill has emptied the record.</summary>
    public MorphBallEyeAiFunction? Function { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        IsActivated = false;
        Angle = RotationAngle.FromTableIndex(0);
        FunctionTimer = WrappingCountdown.Start(0);
        Function = null;
    }
}
