using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The state of one Ceres steam vent. Its function and activation delay are values the game keeps in the enemy's
/// common record, so they are emptied with the record when the vent is killed (<see cref="ClearWithRecord"/>).
/// </summary>
internal sealed class CeresSteamEnemyState
{
    /// <summary>The main-AI function the vent's variant selects; none once a kill has emptied the record.</summary>
    public CeresSteamFunction? Function { get; internal set; }

    /// <summary>The staggered updates before the vent first appears, counted down by its program.</summary>
    public WrappingCountdown ActivationDelay { get; internal set; }

    /// <summary>Where the vent's typed program resumes, set when the vent initializes; a kill leaves it in place.</summary>
    internal AnimationProgramCursor<CeresSteamProgram> Program { get; set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        ActivationDelay = WrappingCountdown.Start(0);
    }
}
