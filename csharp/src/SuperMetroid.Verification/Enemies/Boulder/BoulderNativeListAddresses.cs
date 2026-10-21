using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Boulder;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>
/// The native bank-$A6 lists the Boulder ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class BoulderNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<BoulderProgram>[] Entries =
    [
        new NativeProgramEntry<BoulderProgram>(BoulderProgram.Left, 0x86a7),
        new NativeProgramEntry<BoulderProgram>(BoulderProgram.Right, 0x86cb),
    ];

}
