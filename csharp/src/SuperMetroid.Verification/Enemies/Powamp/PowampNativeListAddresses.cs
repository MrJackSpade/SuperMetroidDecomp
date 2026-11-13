using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Powamp;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>
/// The native bank-$A8 lists the Powamp ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PowampNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PowampSegment>[] Entries =
    [
        new NativeProgramEntry<PowampSegment>(PowampSegment.BodyFast, 0xc163),
        new NativeProgramEntry<PowampSegment>(PowampSegment.BodySlow, 0xc173),
        new NativeProgramEntry<PowampSegment>(PowampSegment.BalloonInflate0, 0xc183),
        new NativeProgramEntry<PowampSegment>(PowampSegment.BalloonInflate1, 0xc187),
        new NativeProgramEntry<PowampSegment>(PowampSegment.BalloonStartSinking, 0xc191),
        new NativeProgramEntry<PowampSegment>(PowampSegment.BalloonDeflated, 0xc199),
    ];

}
