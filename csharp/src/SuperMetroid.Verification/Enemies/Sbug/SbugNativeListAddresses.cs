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
    internal static readonly (SbugProgram Program, ushort Entry)[] Entries =
    [
        (SbugProgram.Right, 0xa071),
        (SbugProgram.UpRight, 0xa085),
        (SbugProgram.Up, 0xa099),
        (SbugProgram.UpLeft, 0xa0ad),
        (SbugProgram.Left, 0xa0c1),
        (SbugProgram.DownLeft, 0xa0d5),
        (SbugProgram.Down, 0xa0e9),
        (SbugProgram.DownRight, 0xa0fd),
    ];

}
