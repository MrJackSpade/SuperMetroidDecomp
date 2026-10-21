using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The state of a Skree. The values the cartridge keeps in the enemy's common record are emptied with it when the enemy
/// is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class SkreeEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<SkreeMetareeProgram> Program { get; set; }

    /// <summary>Reloaded to 21 while diving and counted down while burrowing.</summary>
    public WrappingCountdown BurrowTimer { get; internal set; }

    /// <summary>The literal bank-$A3 phase-function pointer; none once a kill has emptied the record.</summary>
    public SkreeEnemyFunction? Function { get; internal set; }

    /// <summary>The animation phase requested by the current AI transition.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionIndex { get; internal set; }

    /// <summary>The animation phase whose instruction list is currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionIndex { get; internal set; }

    /// <summary>The wind-up instruction's one-shot authorization to begin diving.</summary>
    public bool AttackReady { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        BurrowTimer = WrappingCountdown.Start(0);
        Function = null;
        RequestedInstructionIndex = default;
        InstalledInstructionIndex = default;
        AttackReady = false;
    }
}
