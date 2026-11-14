using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Verification's native view of the grapple connection identities: the bank-$9B words naming them.</summary>
internal static class SamusGrappleConnectionNative
{
    /// <summary>The routine a connection record's function word names.</summary>
    internal static readonly NativeWordIdentities<GrappleConnectionKind> Kinds = new(new Dictionary<GrappleConnectionKind, ushort>
    {
        [GrappleConnectionKind.LockedInPlace] = 0xc77e,
        [GrappleConnectionKind.Swinging] = 0xc79d,
        [GrappleConnectionKind.WallGrab] = 0xc814,
    });

    /// <summary>The pose routine a connection record's handler word names.</summary>
    internal static readonly NativeWordIdentities<GrappleConnectionPose> Poses = new(new Dictionary<GrappleConnectionPose, ushort>
    {
        [GrappleConnectionPose.SwingClockwise] = 0xb9d9,
        [GrappleConnectionPose.SwingAnticlockwise] = 0xb9e2,
        [GrappleConnectionPose.StandingUpRight] = 0xb9ea,
        [GrappleConnectionPose.StandingRight] = 0xb9f3,
        [GrappleConnectionPose.StandingDown] = 0xb9fc,
        [GrappleConnectionPose.StandingUpLeft] = 0xba05,
        [GrappleConnectionPose.CrouchingUpRight] = 0xba0e,
        [GrappleConnectionPose.CrouchingRight] = 0xba17,
        [GrappleConnectionPose.CrouchingDownLeft] = 0xba20,
        [GrappleConnectionPose.CrouchingUpLeft] = 0xba29,
    });
}
