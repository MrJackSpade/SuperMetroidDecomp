using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.MorphBallEye;

namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>
/// The native bank-$A8 lists the Morph Ball eye ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MorphBallEyeNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MorphBallEyeProgram>[] Entries =
    [
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.Active, 0x8fac),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.FacingRightDeactivating, 0x8ff0),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.FacingRightClosed, 0x8ffc),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.FacingLeftDeactivating, 0x9002),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.FacingLeftClosed, 0x900e),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.FacingRightActivating, 0x9014),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.FacingLeftActivating, 0x9026),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.MountFacingRight, 0x9038),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.MountFacingDown, 0x903e),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.MountFacingLeft, 0x9044),
        new NativeProgramEntry<MorphBallEyeProgram>(MorphBallEyeProgram.MountFacingUp, 0x904a),
    ];

}
