using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The state of one Fune or Namihe, the two species that share this actor. The function, wake distance, variant,
/// cooldown and its time are values the cartridge keeps in the enemy's common record, so they are emptied with that
/// record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class FuneNamiheEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<FuneNamiheProgram> Program { get; set; }

    /// <summary>Whether the spitting animation is selected rather than the idle one; waking selects it and the spit's finishing cue deselects it.</summary>
    public bool ShowsActiveAnimation { get; internal set; }

    /// <summary>Whether the right-facing animations are selected, from population parameter one's facing bits.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>The waiting or spitting behaviour; none once a kill has emptied the record.</summary>
    public FuneNamiheEnemyFunction? Function { get; internal set; }

    /// <summary>Strict vertical wake distance in pixels used only by Namihe, population parameter two's high byte (0 through 255).</summary>
    public int YProximity { get; internal set; }

    /// <summary>
    /// The complete low nibble of population parameter one, 0 through 15. Zero selects Fune; every nonzero value
    /// selects Namihe, exactly as the native BEQ test does.
    /// </summary>
    public int VariantIndex { get; internal set; }

    /// <summary>
    /// Fune's incrementing cooldown count of waiting updates, 0 through 65535, compared with <see cref="CooldownTime"/>
    /// as a signed word difference and reset to zero when the wait ends; the waiting routine's increment wraps it at
    /// the word boundary.
    /// </summary>
    public int CooldownTimer { get; internal set; }

    /// <summary>The number of updates Fune waits, initialized from parameter one's high byte.</summary>
    public int CooldownTime { get; internal set; }

    /// <summary>The shared initializer treats any nonzero variant nibble as Namihe.</summary>
    public bool IsNamihe => VariantIndex != 0;

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record. The
    /// animation selection is left as it is: nothing selects an animation for an emptied record.
    /// </summary>
    internal void ClearWithRecord()
    {
        Function = null;
        YProximity = 0;
        VariantIndex = 0;
        CooldownTimer = 0;
        CooldownTime = 0;
    }
}
