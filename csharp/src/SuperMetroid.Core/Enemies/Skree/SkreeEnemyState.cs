using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The state of a Skree. The values the game keeps in the enemy's common record are emptied with it when the enemy
/// is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class SkreeEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<SkreeMetareeProgram> Program { get; set; }

    /// <summary>Reloaded to 21 while diving and counted down while burrowing.</summary>
    public WrappingCountdown BurrowTimer { get; internal set; }

    /// <summary>The literal phase-function pointer; none once a kill has emptied the record.</summary>
    public SkreeEnemyFunction? Function { get; internal set; }

    /// <summary>The animation phase requested by the current AI transition.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionIndex { get; internal set; }

    /// <summary>The animation phase whose instruction list is currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionIndex { get; internal set; }

    /// <summary>The wind-up instruction's one-shot authorization to begin diving.</summary>
    public bool AttackReady { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        BurrowTimer = WrappingCountdown.Start(0);
        Function = null;
        RequestedInstructionIndex = default;
        InstalledInstructionIndex = default;
        AttackReady = false;
    }
}
