namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Lower Norfair Rio's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="LowerNorfairRioAnimations"/>.
/// </summary>
internal enum LowerNorfairRioProgram
{
    /// <summary><c>InstList_Holtz_Idle_0</c> at $A2:C61A.</summary>
    Idle,
    /// <summary>The loop at $A2:C61C.</summary>
    IdleLoop,
    /// <summary><c>InstList_Holtz_PrepareToSwoop</c> at $A2:C630.</summary>
    PrepareToSwoop,
    /// <summary><c>InstList_Holtz_Swoop_Descending</c> at $A2:C65A.</summary>
    Descending,
    /// <summary><c>InstList_Holtz_Swoop_Ascending_Part1</c> at $A2:C662.</summary>
    AscendingPart1,
    /// <summary><c>InstList_Holtz_Swoop_Part2_0</c> at $A2:C674.</summary>
    AscendingPart2,
    /// <summary>The loop at $A2:C676.</summary>
    AscendingPart2Loop,
    /// <summary><c>InstList_Holtz_SwoopCooldown</c> at $A2:C686.</summary>
    Cooldown,
    /// <summary><c>InstList_Holtz_Flames</c> at $A2:C6B0.</summary>
    Flames,
}
