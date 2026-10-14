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
    internal static readonly (MorphBallEyeProgram Program, ushort Entry)[] Entries =
    [
        (MorphBallEyeProgram.Active, 0x8fac),
        (MorphBallEyeProgram.FacingRightDeactivating, 0x8ff0),
        (MorphBallEyeProgram.FacingRightClosed, 0x8ffc),
        (MorphBallEyeProgram.FacingLeftDeactivating, 0x9002),
        (MorphBallEyeProgram.FacingLeftClosed, 0x900e),
        (MorphBallEyeProgram.FacingRightActivating, 0x9014),
        (MorphBallEyeProgram.FacingLeftActivating, 0x9026),
        (MorphBallEyeProgram.MountFacingRight, 0x9038),
        (MorphBallEyeProgram.MountFacingDown, 0x903e),
        (MorphBallEyeProgram.MountFacingLeft, 0x9044),
        (MorphBallEyeProgram.MountFacingUp, 0x904a),
    ];

}
