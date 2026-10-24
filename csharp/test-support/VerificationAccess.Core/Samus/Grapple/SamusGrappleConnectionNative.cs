using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Verification's native view of the grapple connection identities: the bank-$9B words naming them.</summary>
internal static class SamusGrappleConnectionNative
{
    /// <summary>The routine a connection record's function word names.</summary>
    internal static readonly NativeWordIdentities<GrappleConnectionFunction> Functions = new(new Dictionary<GrappleConnectionFunction, ushort>
    {
        [GrappleConnectionFunction.LockedInPlace] = 0xc77e,
        [GrappleConnectionFunction.Swinging] = 0xc79d,
        [GrappleConnectionFunction.WallGrab] = 0xc814,
    });

    /// <summary>The handler a connection record's handler word names.</summary>
    internal static readonly NativeWordIdentities<GrappleConnectionHandler> Handlers = new(new Dictionary<GrappleConnectionHandler, ushort>
    {
        [GrappleConnectionHandler.SwingClockwise] = 0xb9d9,
        [GrappleConnectionHandler.SwingAnticlockwise] = 0xb9e2,
        [GrappleConnectionHandler.StandingUpRight] = 0xb9ea,
        [GrappleConnectionHandler.StandingRight] = 0xb9f3,
        [GrappleConnectionHandler.StandingDown] = 0xb9fc,
        [GrappleConnectionHandler.StandingUpLeft] = 0xba05,
        [GrappleConnectionHandler.CrouchingUpRight] = 0xba0e,
        [GrappleConnectionHandler.CrouchingRight] = 0xba17,
        [GrappleConnectionHandler.CrouchingDownLeft] = 0xba20,
        [GrappleConnectionHandler.CrouchingUpLeft] = 0xba29,
    });
}
