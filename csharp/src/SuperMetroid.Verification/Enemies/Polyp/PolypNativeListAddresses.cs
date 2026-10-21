using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Polyp;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>
/// The native bank-$A2 lists the Polyp ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PolypNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PolypProgram>[] Entries =
    [
        new NativeProgramEntry<PolypProgram>(PolypProgram.Stationary, 0xb51a),
    ];

}
