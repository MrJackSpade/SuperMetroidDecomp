using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.Verification.Enemies.Sbug;

/// <summary>
/// The native bank-$A3 lists the Sbug ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class SbugNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>
    /// Each facing's native entry point, in the order <see cref="CompassDirection"/> declares the facings its loops are keyed
    /// by (#2557); the cartridge stores them from facing right, turning counterclockwise.
    /// </summary>
    internal static readonly NativeProgramEntry<CompassDirection>[] Entries =
    [
        new NativeProgramEntry<CompassDirection>(CompassDirection.Up, 0xa099),
        new NativeProgramEntry<CompassDirection>(CompassDirection.UpRight, 0xa085),
        new NativeProgramEntry<CompassDirection>(CompassDirection.Right, 0xa071),
        new NativeProgramEntry<CompassDirection>(CompassDirection.DownRight, 0xa0fd),
        new NativeProgramEntry<CompassDirection>(CompassDirection.Down, 0xa0e9),
        new NativeProgramEntry<CompassDirection>(CompassDirection.DownLeft, 0xa0d5),
        new NativeProgramEntry<CompassDirection>(CompassDirection.Left, 0xa0c1),
        new NativeProgramEntry<CompassDirection>(CompassDirection.UpLeft, 0xa0ad),
    ];

}
