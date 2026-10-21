using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The state of one Metaree. Every value here is one the cartridge keeps in the enemy's common record, so it is
/// emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class MetareeEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<SkreeMetareeProgram> Program { get; set; }

    /// <summary>Reloaded to 21 during flight, then counted down while underground.</summary>
    public WrappingCountdown BurrowTimer { get; internal set; }

    /// <summary>The idle, preparing, diving or burrowing behaviour; none once a kill has emptied the record.</summary>
    public MetareeEnemyFunction? Function { get; internal set; }

    /// <summary>Animation phase requested by AI.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionListIndex { get; internal set; }

    /// <summary>Animation phase currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionListIndex { get; internal set; }

    /// <summary>Set by the launch crouch's finishing cue.</summary>
    public bool AttackReady { get; internal set; }

    /// <summary>
    /// Whole-pixel downward speed calculated once from Samus's vertical position: the wrapped (unsigned) vertical
    /// distance divided by 24, plus four, so 4 through 2734. A Samus above the Metaree wraps the distance and gives
    /// the original game's enormous dive.
    /// </summary>
    public int YVelocity { get; internal set; }

    /// <summary>Empties every value, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        BurrowTimer = WrappingCountdown.Start(0);
        Function = null;
        RequestedInstructionListIndex = SkreeMetareeAnimationPhase.Idling;
        InstalledInstructionListIndex = SkreeMetareeAnimationPhase.Idling;
        AttackReady = false;
        YVelocity = 0;
    }
}
