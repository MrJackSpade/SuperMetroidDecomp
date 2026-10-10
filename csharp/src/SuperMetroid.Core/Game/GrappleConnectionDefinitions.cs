using static SuperMetroid.Core.Game.SamusGrappleRomData.Connections;
using static SuperMetroid.Core.Game.SamusPoseId;

namespace SuperMetroid.Core.Game;

/// <summary>Native Grapple connection, cancellation and dropped-pose mechanics, not visual assets.</summary>
internal static class GrappleConnectionDefinitions
{
    /// <summary>
    /// $9B:C43E GrappleBeamSpecialAngles stores eight exact collision-stop records. They are four
    /// authored stops, each immediately followed by its left/right reflection: the angle reflects
    /// as $0100 - angle, X negates and the pose swaps facing; Y and the next function are shared.
    /// </summary>
    private static readonly SpecialConnection[] AuthoredStops =
    [
        new(0xd680, GrappleCrouchingDownRightPose, -30, -24, LockedInPlaceHandler),
        new(0xb380, GrappleCrouchingDownRightPose, -28, -8, LockedInPlaceHandler),
        new(0x6a80, GrappleWallContactRightPose, 24, 16, WallGrabHandler),
        new(0x7380, GrappleWallContactLeftPose, -8, 16, WallGrabHandler),
    ];

    internal readonly record struct SpecialConnection(ushort Angle, SamusPoseId Pose, short X, short Y, ushort Function);

    /// <summary>Number of native special-angle records, in their native order.</summary>
    internal static int SpecialAngleCount => AuthoredStops.Length * 2;

    /// <summary>The native record at <paramref name="index"/>: an authored stop or the reflection that follows it.</summary>
    internal static SpecialConnection SpecialAngle(int index)
    {
        if ((uint)index >= SpecialAngleCount) throw new IndexOutOfRangeException();
        SpecialConnection stop = AuthoredStops[index >> 1];
        return (index & 1) == 0 ? stop : new(unchecked((ushort)(0x0100 - stop.Angle)), MirrorPose(stop.Pose),
            (short)-stop.X, stop.Y, stop.Function);
    }

    private static SamusPoseId MirrorPose(SamusPoseId pose) => pose switch
    {
        GrappleCrouchingDownRightPose => GrappleCrouchingDownLeftPose,
        GrappleCrouchingDownLeftPose => GrappleCrouchingDownRightPose,
        GrappleWallContactRightPose => GrappleWallContactLeftPose,
        GrappleWallContactLeftPose => GrappleWallContactRightPose,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };
    /// <summary>$9B:B8B8 cancellation policy for the 28 movement dispatch identities.</summary>
    internal static bool CancelsFiring(SamusMovementType movement) => movement switch
    {
        SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping
            or SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Unused0B
            or SamusMovementType.Unused0C or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall
            or SamusMovementType.Grappling or SamusMovementType.DraygonHeld => false,
        >= SamusMovementType.Standing and <= SamusMovementType.Special => true,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$9B:C9BA/$C9C4 directional standing/crouching poses after the rope drops.</summary>
    internal static SamusPoseId DroppedPose(byte direction, bool compact) => direction switch
    {
        0 => compact ? CrouchingAimUpRightPose : StandingAimUpRightPose,
        1 => compact ? CrouchingAimDiagonalUpRightPose : StandingAimDiagonalUpRightPose,
        2 or 4 => compact ? CrouchingRightPose : FacingRightNormalPose,
        3 => compact ? CrouchingAimDiagonalDownRightPose : StandingAimDiagonalDownRightPose,
        5 or 7 => compact ? CrouchingLeftPose : FacingLeftNormalPose,
        6 => compact ? CrouchingAimDiagonalDownLeftPose : StandingAimDiagonalDownLeftPose,
        8 => compact ? CrouchingAimDiagonalUpLeftPose : StandingAimDiagonalUpLeftPose,
        9 => compact ? CrouchingAimUpLeftPose : StandingAimUpLeftPose,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$9B:C3C6/$C3EE/$C416 default, vertical and crouching directional connection policies.</summary>
    internal static (ushort Function, ushort Handler) ResolveConnection(int address)
    {
        int offset = address - DefaultTable;
        if (offset < 0 || offset >= 30 * 4 || (offset & 3) != 0)
            throw new InvalidDataException(
                $"Grapple connection record ${address:X6} is outside the compiled definitions.");
        int record = offset / 4;
        int direction = record % 10;
        bool compact = record >= 20;
        ushort handler = record is >= 10 and < 20
            ? direction < 5 ? SwingClockwiseHandler : SwingAnticlockwiseHandler
            : direction switch
            {
                0 or 1 => SwingClockwiseHandler,
                2 => compact ? CrouchingUpRightHandler : StandingUpRightHandler,
                3 => compact ? CrouchingRightHandler : StandingRightHandler,
                4 or 5 => StandingDownHandler,
                6 => compact ? CrouchingDownLeftHandler : StandingDownHandler,
                7 => compact ? CrouchingUpLeftHandler : StandingUpLeftHandler,
                _ => SwingAnticlockwiseHandler,
            };
        ushort function = handler is SwingClockwiseHandler or SwingAnticlockwiseHandler ? SwingingHandler : LockedInPlaceHandler;
        return (function, handler);
    }
}
