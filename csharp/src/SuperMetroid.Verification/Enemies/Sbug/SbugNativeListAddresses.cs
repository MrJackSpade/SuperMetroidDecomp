using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Sbug;

namespace SuperMetroid.Verification.Enemies.Sbug;

/// <summary>
/// The native bank-$A3 lists the Sbug ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class SbugNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SbugProgram>[] Entries =
    [
        new NativeProgramEntry<SbugProgram>(SbugProgram.Right, 0xa071),
        new NativeProgramEntry<SbugProgram>(SbugProgram.UpRight, 0xa085),
        new NativeProgramEntry<SbugProgram>(SbugProgram.Up, 0xa099),
        new NativeProgramEntry<SbugProgram>(SbugProgram.UpLeft, 0xa0ad),
        new NativeProgramEntry<SbugProgram>(SbugProgram.Left, 0xa0c1),
        new NativeProgramEntry<SbugProgram>(SbugProgram.DownLeft, 0xa0d5),
        new NativeProgramEntry<SbugProgram>(SbugProgram.Down, 0xa0e9),
        new NativeProgramEntry<SbugProgram>(SbugProgram.DownRight, 0xa0fd),
    ];

}
