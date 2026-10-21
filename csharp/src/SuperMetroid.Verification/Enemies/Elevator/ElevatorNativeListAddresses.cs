using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>
/// The native bank-$A3 lists the elevator ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ElevatorNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ElevatorProgram>[] Entries =
    [
        new NativeProgramEntry<ElevatorProgram>(ElevatorProgram.Loop, 0x94d6),
    ];

}
