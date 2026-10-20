using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The state of the Kago. Its behaviour, animation speed and death latch are values the cartridge keeps in the enemy's
/// common record, so a kill empties them with that record (<see cref="ClearWithRecord"/>); its hit counter lives
/// outside the record and outlives the kill.
/// </summary>
public sealed class KagoEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<KagoProgram> Program { get; set; }

    /// <summary>The current behaviour, installed at initialization and replaced on the first AI update; none once a kill has emptied the record.</summary>
    public KagoEnemyFunction? Function { get; internal set; }

    /// <summary>Whether the shell runs its three-frame loop rather than its ten-frame one.</summary>
    public bool UsesFastAnimation { get; internal set; }

    /// <summary>
    /// Extra enemy word four. The ROM decrements first and dies only when the signed result
    /// is negative, so population value ten permits eleven accepted shots.
    /// </summary>
    public NativeCountdown HitCounter { get; internal set; }

    /// <summary>Set after Kago requests death animation variant four.</summary>
    public bool DeathAnimationStarted { get; internal set; }

    /// <summary>Number of bank-$86 Kago bugs successfully allocated from this shell.</summary>
    public int SpawnedBugCount { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no behaviour, the slow loop, no death.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        UsesFastAnimation = false;
        DeathAnimationStarted = false;
    }
}
