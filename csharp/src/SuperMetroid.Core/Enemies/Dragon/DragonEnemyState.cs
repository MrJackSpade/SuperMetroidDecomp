using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The state of one Dragon body or cosmetic wing. The game reuses one record word first as the body's motion
/// timer and then as its three-shot counter; here each role has its own countdown, since every phase starts its own
/// before reading it. The facing, countdowns, function and wing selection are values the game keeps in the
/// enemy's common record, so they are emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class DragonEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<DragonProgram> Program { get; set; }

    /// <summary>Population parameter one: false is a body; true is its cosmetic wing.</summary>
    public bool IsWing { get; internal set; }

    /// <summary>Whether the body last turned to face left, toward a Samus on its left; the fireball spawns on that side.</summary>
    public bool FacingLeft { get; internal set; }

    /// <summary>The body's countdown while waiting, rising, or sinking.</summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>The body's shots left while attacking; initialized to three before the first volley.</summary>
    public int AttackCounter { get; internal set; }

    /// <summary>The current behaviour; none once a kill has emptied the record.</summary>
    public DragonEnemyFunction? Function { get; internal set; }

    /// <summary>The animation main AI asks for.</summary>
    public DragonAnimationSelector RequestedAnimation { get; internal set; }

    /// <summary>The animation already installed; a request for it installs nothing unless <see cref="ReinstallRequested"/>.</summary>
    public DragonAnimationSelector InstalledAnimation { get; internal set; }

    /// <summary>
    /// Whether the next install restarts the requested animation even when it is already installed: each fireball
    /// restarts the attack, which spaces the three fireballs by a whole attack animation.
    /// </summary>
    public bool ReinstallRequested { get; internal set; }

    /// <summary>Set by the attack animation's finishing cue.</summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        IsWing = false;
        FacingLeft = false;
        FunctionTimer = 0;
        AttackCounter = 0;
        Function = null;
    }
}
