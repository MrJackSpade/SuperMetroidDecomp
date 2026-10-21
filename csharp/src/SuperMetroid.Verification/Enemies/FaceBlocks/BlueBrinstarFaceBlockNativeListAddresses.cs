using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<BlueBrinstarFaceBlockProgram>[] Entries =
    [
        new NativeProgramEntry<BlueBrinstarFaceBlockProgram>(BlueBrinstarFaceBlockProgram.SamusLeft, 0xe80c),
        new NativeProgramEntry<BlueBrinstarFaceBlockProgram>(BlueBrinstarFaceBlockProgram.SamusRight, 0xe81a),
        new NativeProgramEntry<BlueBrinstarFaceBlockProgram>(BlueBrinstarFaceBlockProgram.Initial, 0xe828),
    ];

}
