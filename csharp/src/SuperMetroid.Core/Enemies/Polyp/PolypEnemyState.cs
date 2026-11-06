using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// The state of the Polyp. Its behaviour and launch cooldown are values the game keeps in the enemy's common
/// record, so a kill empties them with that record (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class PolypEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<PolypProgram> Program { get; set; }

    /// <summary>The current Polyp behaviour; none once a kill has emptied the record.</summary>
    public PolypEnemyFunction? Function { get; internal set; }

    /// <summary>The launch cooldown, decremented through -1 before the Polyp rearms.</summary>
    public int CooldownTimer { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record: no behaviour and no cooldown.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        CooldownTimer = 0;
    }
}
