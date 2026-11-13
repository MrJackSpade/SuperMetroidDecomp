using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<MorphBallEyeSegment>[] Entries =
    [
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.Active, 0x8fac),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.FacingRightDeactivating, 0x8ff0),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.FacingRightClosed, 0x8ffc),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.FacingLeftDeactivating, 0x9002),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.FacingLeftClosed, 0x900e),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.FacingRightActivating, 0x9014),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.FacingLeftActivating, 0x9026),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.MountFacingRight, 0x9038),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.MountFacingDown, 0x903e),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.MountFacingLeft, 0x9044),
        new NativeProgramEntry<MorphBallEyeSegment>(MorphBallEyeSegment.MountFacingUp, 0x904a),
    ];

}
