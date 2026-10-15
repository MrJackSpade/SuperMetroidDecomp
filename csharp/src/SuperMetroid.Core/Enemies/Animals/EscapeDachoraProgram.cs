namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed programs (#641), one per bank-$B3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="EscapeDachoraAnimations"/>.
/// </summary>
internal enum EscapeDachoraProgram
{
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_0</c> at $B3:E964.</summary>
    RunningAroundLowTide,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_1</c> at $B3:E968.</summary>
    RunningAroundLowTideLeft,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_2</c> at $B3:E99C.</summary>
    RunningAroundLowTideRight,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_0</c> at $B3:E9D0.</summary>
    RunningAroundHighTide,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_1</c> at $B3:E9D4.</summary>
    RunningAroundHighTideLeft,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_2</c> at $B3:E9FC.</summary>
    RunningAroundHighTideLeftLoop,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_3</c> at $B3:EA04.</summary>
    RunningAroundHighTideRight,
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_4</c> at $B3:EA2C.</summary>
    RunningAroundHighTideRightLoop,
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_0</c> at $B3:EA34.</summary>
    RunningForEscape,
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_1</c> at $B3:EA38.</summary>
    RunningForEscapeAccelerating,
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_2</c> at $B3:EA80.</summary>
    RunningForEscapeMaximumSpeed,
}
