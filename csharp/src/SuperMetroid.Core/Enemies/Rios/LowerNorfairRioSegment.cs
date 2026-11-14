namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Lower Norfair Rio's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="LowerNorfairRioAnimations"/>.
/// </summary>
internal enum LowerNorfairRioSegment
{
    /// <summary>The idle animation.</summary>
    Idle,
    /// <summary>The loop.</summary>
    IdleLoop,
    /// <summary>The prepare to swoop animation.</summary>
    PrepareToSwoop,
    /// <summary>The descending animation.</summary>
    Descending,
    /// <summary>The opening of the climb back to the perch, which holds its last pose until its cue.</summary>
    AscentOpening,
    /// <summary>The climb back to the perch, running on into its loop.</summary>
    Ascending,
    /// <summary>The loop.</summary>
    AscendingLoop,
    /// <summary>The cooldown animation.</summary>
    Cooldown,
    /// <summary>The flames animation.</summary>
    Flames,
}
