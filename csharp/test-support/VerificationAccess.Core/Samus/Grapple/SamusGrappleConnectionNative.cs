using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Verification's native view of the grapple connection identities: the bank-$9B words naming them.</summary>
internal static class SamusGrappleConnectionNative
{
    /// <summary>The routine a connection record's function word names.</summary>
    internal static readonly NativeWordIdentities<GrappleConnectionKind> Kinds = new(new Dictionary<GrappleConnectionKind, ushort>
    {
        [GrappleConnectionKind.LockedInPlace] = 0xc77e,
        [GrappleConnectionKind.Swinging] = 0xc79d,
    });

    /// <summary>The pose a connection record's handler word installs: the routine that sets that pose.</summary>
    internal static readonly NativeWordIdentities<SamusPoseId> Poses = new(new Dictionary<SamusPoseId, ushort>
    {
        [SamusPoseId.GrappleSwingRightPose] = 0xb9d9,
        [SamusPoseId.GrappleSwingLeftPose] = 0xb9e2,
        [SamusPoseId.GrappleStandingRightPose] = 0xb9ea,
        [SamusPoseId.GrappleStandingDownRightPose] = 0xb9f3,
        [SamusPoseId.GrappleStandingDownLeftPose] = 0xb9fc,
        [SamusPoseId.GrappleStandingLeftPose] = 0xba05,
        [SamusPoseId.GrappleCrouchingRightPose] = 0xba0e,
        [SamusPoseId.GrappleCrouchingDownRightPose] = 0xba17,
        [SamusPoseId.GrappleCrouchingDownLeftPose] = 0xba20,
        [SamusPoseId.GrappleCrouchingLeftPose] = 0xba29,
    });
}
