using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.FaceBlocks;

namespace SuperMetroid.Verification.Enemies.FaceBlocks;

/// <summary>
/// The native bank-$A8 lists the Blue Brinstar face block ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class BlueBrinstarFaceBlockNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<BlueBrinstarFaceBlockSegment>[] Entries =
    [
        new NativeProgramEntry<BlueBrinstarFaceBlockSegment>(BlueBrinstarFaceBlockSegment.SamusLeft, 0xe80c),
        new NativeProgramEntry<BlueBrinstarFaceBlockSegment>(BlueBrinstarFaceBlockSegment.SamusRight, 0xe81a),
        new NativeProgramEntry<BlueBrinstarFaceBlockSegment>(BlueBrinstarFaceBlockSegment.Initial, 0xe828),
    ];

}
