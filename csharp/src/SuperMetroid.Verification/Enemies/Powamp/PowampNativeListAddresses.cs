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

    /// <summary>Each of the body's typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PowampBodySegment>[] BodyEntries =
    [
        new NativeProgramEntry<PowampBodySegment>(PowampBodySegment.BodyFast, 0xc163),
        new NativeProgramEntry<PowampBodySegment>(PowampBodySegment.BodySlow, 0xc173),
    ];

    /// <summary>Each of the balloon's typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PowampBalloonSegment>[] BalloonEntries =
    [
        new NativeProgramEntry<PowampBalloonSegment>(PowampBalloonSegment.InflatingFromDeflated, 0xc183),
        new NativeProgramEntry<PowampBalloonSegment>(PowampBalloonSegment.InflatingFromHalfway, 0xc187),
        new NativeProgramEntry<PowampBalloonSegment>(PowampBalloonSegment.StartSinking, 0xc191),
        new NativeProgramEntry<PowampBalloonSegment>(PowampBalloonSegment.Deflated, 0xc199),
    ];

}
