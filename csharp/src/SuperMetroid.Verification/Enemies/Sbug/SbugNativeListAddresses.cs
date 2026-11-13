using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<SbugSegment>[] Entries =
    [
        new NativeProgramEntry<SbugSegment>(SbugSegment.Right, 0xa071),
        new NativeProgramEntry<SbugSegment>(SbugSegment.UpRight, 0xa085),
        new NativeProgramEntry<SbugSegment>(SbugSegment.Up, 0xa099),
        new NativeProgramEntry<SbugSegment>(SbugSegment.UpLeft, 0xa0ad),
        new NativeProgramEntry<SbugSegment>(SbugSegment.Left, 0xa0c1),
        new NativeProgramEntry<SbugSegment>(SbugSegment.DownLeft, 0xa0d5),
        new NativeProgramEntry<SbugSegment>(SbugSegment.Down, 0xa0e9),
        new NativeProgramEntry<SbugSegment>(SbugSegment.DownRight, 0xa0fd),
    ];

}
