namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="RioAnimations"/>.
/// </summary>
internal enum RioProgram
{
    /// <summary><c>InstList_Rio_Idle</c> at $A2:BB4B.</summary>
    Idle,
    /// <summary><c>InstList_Rio_PostSwoopIdle</c> at $A2:BB53.</summary>
    PostSwoopIdle,
    /// <summary><c>InstList_Rio_Swooping_Part1</c> at $A2:BB7F.</summary>
    SwoopingPart1,
    /// <summary><c>InstList_Rio_Swooping_Part2</c> at $A2:BB97.</summary>
    SwoopingPart2,
    /// <summary><c>InstList_Rio_SwoopCooldown</c> at $A2:BBA3.</summary>
    SwoopCooldown,
}
