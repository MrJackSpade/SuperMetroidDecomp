using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Ripper;

namespace SuperMetroid.Verification.Enemies.Ripper;

/// <summary>
/// The native bank-$A2 lists the Ripper ROM oracle (#1290) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class RipperNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<RipperSegment>[] Entries =
    [
        new NativeProgramEntry<RipperSegment>(RipperSegment.GRipperMovingLeft, 0xe19b),
        new NativeProgramEntry<RipperSegment>(RipperSegment.GRipperMovingRight, 0xe1af),
        new NativeProgramEntry<RipperSegment>(RipperSegment.Ripper2MovingRight, 0xe2e0),
        new NativeProgramEntry<RipperSegment>(RipperSegment.Ripper2MovingLeft, 0xe2f4),
        new NativeProgramEntry<RipperSegment>(RipperSegment.RipperMovingRight, 0xe477),
        new NativeProgramEntry<RipperSegment>(RipperSegment.RipperMovingLeft, 0xe48b),
    ];

}
