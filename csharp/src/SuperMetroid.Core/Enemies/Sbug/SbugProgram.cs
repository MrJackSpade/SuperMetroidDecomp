namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="SbugAnimations"/>.
/// </summary>
internal enum SbugProgram
{
    /// <summary>The native list at $A3:A071.</summary>
    Right,
    /// <summary>The native list at $A3:A085.</summary>
    UpRight,
    /// <summary>The native list at $A3:A099.</summary>
    Up,
    /// <summary>The native list at $A3:A0AD.</summary>
    UpLeft,
    /// <summary>The native list at $A3:A0C1.</summary>
    Left,
    /// <summary>The native list at $A3:A0D5.</summary>
    DownLeft,
    /// <summary>The native list at $A3:A0E9.</summary>
    Down,
    /// <summary>The native list at $A3:A0FD.</summary>
    DownRight,
}
