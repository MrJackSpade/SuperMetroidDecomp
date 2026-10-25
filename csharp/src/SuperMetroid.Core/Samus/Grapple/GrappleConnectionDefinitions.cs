using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionHandler;
using static SuperMetroid.Core.Samus.Poses.SamusPoseId;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

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
        new(RotationAngle.FromTableIndex(0xd6, 0x80), GrappleCrouchingDownRightPose, new(-30, -24), GrappleConnectionFunction.LockedInPlace),
        new(RotationAngle.FromTableIndex(0xb3, 0x80), GrappleCrouchingDownRightPose, new(-28, -8), GrappleConnectionFunction.LockedInPlace),
        new(RotationAngle.FromTableIndex(0x6a, 0x80), GrappleWallContactRightPose, new(24, 16), GrappleConnectionFunction.WallGrab),
        new(RotationAngle.FromTableIndex(0x73, 0x80), GrappleWallContactLeftPose, new(-8, 16), GrappleConnectionFunction.WallGrab),
    ];

    /// <summary>One collision-stop record.</summary>
    /// <param name="Angle">The rope angle that triggers the stop.</param>
    /// <param name="Pose">Samus's pose at the stop.</param>
    /// <param name="SamusOffset">Samus's room-pixel position relative to the grapple anchor.</param>
    /// <param name="Function">The connection function installed at the stop.</param>
    internal readonly record struct SpecialConnection(RotationAngle Angle, SamusPoseId Pose, Offset16<RoomPixels> SamusOffset, GrappleConnectionFunction Function);

    /// <summary>Number of native special-angle records, in their native order.</summary>
    internal static int SpecialAngleCount => AuthoredStops.Length * 2;

    /// <summary>The native record at <paramref name="index"/>: an authored stop or the reflection that follows it.</summary>
    internal static SpecialConnection SpecialAngle(int index)
    {
        if ((uint)index >= SpecialAngleCount) throw new ArgumentOutOfRangeException(nameof(index));
        SpecialConnection stop = AuthoredStops[index >> 1];
        return (index & 1) == 0 ? stop : new(RotationAngle.FromRaw(unchecked((ushort)(0x0100 - stop.Angle.RawValue))), SamusPoseMirror.Of(stop.Pose),
            stop.SamusOffset.MirroredX(), stop.Function);
    }

    /// <summary>$9B:B8B8 cancellation policy for the 28 movement dispatch identities.</summary>
    internal static bool CancelsFiring(SamusMovementType movement) => movement switch
    {
        SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping
            or SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Unused0B
            or SamusMovementType.Unused0C or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall
            or SamusMovementType.Grappling or SamusMovementType.DraygonHeld => false,
        SamusMovementType.Standing or
            SamusMovementType.Running or
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.MorphBallGround or
            SamusMovementType.Crouching or
            SamusMovementType.Falling or
            SamusMovementType.UnusedGlitchBall or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.Knockback or
            SamusMovementType.Unused0B or
            SamusMovementType.Unused0C or
            SamusMovementType.Unused0D or
            SamusMovementType.TurningOnGround or
            SamusMovementType.PostureTransition or
            SamusMovementType.Moonwalking or
            SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping or
            SamusMovementType.RanIntoWall or
            SamusMovementType.Grappling or
            SamusMovementType.TurningWhileJumping or
            SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or
            SamusMovementType.DraygonHeld or
            SamusMovementType.Special => true,
        _ => throw new ArgumentOutOfRangeException(nameof(movement)),
    };

    /// <summary>$9B:C9BA/$C9C4 directional standing/crouching poses after the rope drops.</summary>
    internal static SamusPoseId DroppedPose(int direction, bool compact) => direction switch
    {
        0 => compact ? CrouchingAimUpRightPose : StandingAimUpRightPose,
        1 => compact ? CrouchingAimDiagonalUpRightPose : StandingAimDiagonalUpRightPose,
        2 or 4 => compact ? CrouchingRightPose : FacingRightNormalPose,
        3 => compact ? CrouchingAimDiagonalDownRightPose : StandingAimDiagonalDownRightPose,
        5 or 7 => compact ? CrouchingLeftPose : FacingLeftNormalPose,
        6 => compact ? CrouchingAimDiagonalDownLeftPose : StandingAimDiagonalDownLeftPose,
        8 => compact ? CrouchingAimDiagonalUpLeftPose : StandingAimDiagonalUpLeftPose,
        9 => compact ? CrouchingAimUpLeftPose : StandingAimUpLeftPose,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>The connection records each table begins at; a direction past a table's ten reads on into the next's.</summary>
    private static int FirstRecordOf(GrappleConnectionTable table) => table switch
    {
        GrappleConnectionTable.Default => 0,
        GrappleConnectionTable.MovingVertically => 10,
        GrappleConnectionTable.Crouching => 20,
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown grapple connection table."),
    };

    /// <summary>
    /// The default, vertical and crouching directional connection policies ($9B:C3C6/$C3EE/$C416): the record
    /// <paramref name="table"/>'s policy names for <paramref name="fireDirection"/>, the records lying one table after another.
    /// </summary>
    /// <exception cref="InvalidDataException">The direction reaches past the last compiled record.</exception>
    internal static GrappleConnection ResolveConnection(GrappleConnectionTable table, int fireDirection)
    {
        int record = FirstRecordOf(table) + fireDirection;
        if (fireDirection < 0 || record >= 30)
            throw new InvalidDataException($"Grapple connection {table} direction {fireDirection} is outside the compiled definitions.");
        int direction = record % 10;
        bool compact = record >= 20;
        GrappleConnectionHandler handler = record is >= 10 and < 20
            ? direction < 5 ? SwingClockwise : SwingAnticlockwise
            : direction switch
            {
                0 or 1 => SwingClockwise,
                2 => compact ? CrouchingUpRight : StandingUpRight,
                3 => compact ? CrouchingRight : StandingRight,
                4 or 5 => StandingDown,
                6 => compact ? CrouchingDownLeft : StandingDown,
                7 => compact ? CrouchingUpLeft : StandingUpLeft,
                _ => SwingAnticlockwise,
            };
        GrappleConnectionFunction function = handler is SwingClockwise or SwingAnticlockwise
            ? GrappleConnectionFunction.Swinging
            : GrappleConnectionFunction.LockedInPlace;
        return new GrappleConnection(function, handler);
    }
}
