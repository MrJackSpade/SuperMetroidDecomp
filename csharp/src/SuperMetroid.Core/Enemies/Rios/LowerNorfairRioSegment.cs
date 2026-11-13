namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Lower Norfair Rio's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
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
    /// <summary>The ascending part 1 animation.</summary>
    AscendingPart1,
    /// <summary>The ascending part 2 animation.</summary>
    AscendingPart2,
    /// <summary>The loop.</summary>
    AscendingPart2Loop,
    /// <summary>The cooldown animation.</summary>
    Cooldown,
    /// <summary>The flames animation.</summary>
    Flames,
}
