using static SuperMetroid.Core.Game.SamusGrappleRomData.Connections;
using static SuperMetroid.Core.Game.SamusPoseIds;

namespace SuperMetroid.Core.Game;

/// <summary>Native Grapple connection, cancellation and dropped-pose mechanics, not visual assets.</summary>
internal static class GrappleConnectionDefinitions
{
    /// <summary>$9B:B8B8 CancelGrappleBeamIfInIncompatiblePose.poses: cancellation bytes for the 28 movement types.</summary>
    private static ReadOnlySpan<byte> Cancellation =>
        [0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1];

    /// <summary>$9B:C3C6/C3EE/C416 ConnectingToGrappleBlockPointerTable: default, vertical and crouching handler rows.</summary>
    private static ReadOnlySpan<ushort> Handlers =>
    [
        SwingClockwiseHandler, SwingClockwiseHandler, StandingUpRightHandler, StandingRightHandler,
        StandingDownHandler, StandingDownHandler, StandingDownHandler, StandingUpLeftHandler,
        SwingAnticlockwiseHandler, SwingAnticlockwiseHandler,
        SwingClockwiseHandler, SwingClockwiseHandler, SwingClockwiseHandler, SwingClockwiseHandler,
        SwingClockwiseHandler, SwingAnticlockwiseHandler, SwingAnticlockwiseHandler, SwingAnticlockwiseHandler,
        SwingAnticlockwiseHandler, SwingAnticlockwiseHandler,
        SwingClockwiseHandler, SwingClockwiseHandler, CrouchingUpRightHandler, CrouchingRightHandler,
        StandingDownHandler, StandingDownHandler, CrouchingDownLeftHandler, CrouchingUpLeftHandler,
        SwingAnticlockwiseHandler, SwingAnticlockwiseHandler,
    ];

    /// <summary>$9B:C43E GrappleBeamSpecialAngles: exact collision-stop angles, poses, offsets and next function words.</summary>
    private static readonly SpecialConnection[] SpecialAngleRecords =
    [
        new(0xd680, GrappleCrouchingDownRightPose, -30, -24, LockedInPlaceHandler),
        new(0x2a80, GrappleCrouchingDownLeftPose, 30, -24, LockedInPlaceHandler),
        new(0xb380, GrappleCrouchingDownRightPose, -28, -8, LockedInPlaceHandler),
        new(0x4d80, GrappleCrouchingDownLeftPose, 28, -8, LockedInPlaceHandler),
        new(0x6a80, GrappleWallContactRightPose, 24, 16, WallGrabHandler),
        new(0x9680, GrappleWallContactLeftPose, -24, 16, WallGrabHandler),
        new(0x7380, GrappleWallContactLeftPose, -8, 16, WallGrabHandler),
        new(0x8d80, GrappleWallContactRightPose, 8, 16, WallGrabHandler),
    ];

    /// <summary>$9B:C9BA GrappleBeamFunction_Dropped.standingPoses: full-height directional drop poses.</summary>
    private static ReadOnlySpan<byte> StandingDrops =>
    [
        StandingAimUpRightPose, StandingAimDiagonalUpRightPose, FacingRightNormalPose,
        StandingAimDiagonalDownRightPose, FacingRightNormalPose, FacingLeftNormalPose,
        StandingAimDiagonalDownLeftPose, FacingLeftNormalPose, StandingAimDiagonalUpLeftPose, StandingAimUpLeftPose,
    ];

    /// <summary>$9B:C9C4 GrappleBeamFunction_Dropped.crouchingPoses: compact directional drop poses.</summary>
    private static ReadOnlySpan<byte> CrouchingDrops =>
    [
        CrouchingAimUpRightPose, CrouchingAimDiagonalUpRightPose, CrouchingRightPose,
        CrouchingAimDiagonalDownRightPose, CrouchingRightPose, CrouchingLeftPose,
        CrouchingAimDiagonalDownLeftPose, CrouchingLeftPose, CrouchingAimDiagonalUpLeftPose, CrouchingAimUpLeftPose,
    ];

    internal readonly record struct SpecialConnection(ushort Angle, byte Pose, short X, short Y, ushort Function);

    internal static ReadOnlySpan<SpecialConnection> SpecialAngles => SpecialAngleRecords;
    internal static bool CancelsFiring(SamusMovementType movement) => Cancellation[(byte)movement] != 0;
    internal static byte DroppedPose(byte direction, bool compact) => (compact ? CrouchingDrops : StandingDrops)[direction];

    internal static bool TryResolveConnection(int address, out (ushort Function, ushort Handler) connection)
    {
        int offset = address - DefaultTable;
        if (offset < 0 || offset >= Handlers.Length * 4 || (offset & 3) != 0)
        {
            connection = default;
            return false;
        }
        ushort handler = Handlers[offset / 4];
        ushort function = handler is SwingClockwiseHandler or SwingAnticlockwiseHandler ? SwingingHandler : LockedInPlaceHandler;
        connection = (function, handler);
        return true;
    }
}
