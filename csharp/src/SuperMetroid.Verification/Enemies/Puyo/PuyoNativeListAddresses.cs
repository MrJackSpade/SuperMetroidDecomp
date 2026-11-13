using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Puyo;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>
/// The native bank-$A2 lists the Puyo ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PuyoNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PuyoSegment>[] Entries =
    [
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.GroundedFast, 0x99ad),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.GroundedMedium, 0x99c1),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.GroundedSlow, 0x99d5),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.RightFrame0LeftFrame4, 0x99e9),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.RightFrame1LeftFrame3, 0x99ef),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.Frame2, 0x99f5),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.RightFrame3LeftFrame1, 0x99fb),
        new NativeProgramEntry<PuyoSegment>(PuyoSegment.RightFrame4LeftFrame0, 0x9a01),
    ];

}
