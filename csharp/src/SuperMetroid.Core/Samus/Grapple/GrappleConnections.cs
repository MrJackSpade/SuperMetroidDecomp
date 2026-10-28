using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionDefinitions;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionHandler;
using static SuperMetroid.Core.Samus.Poses.SamusPoseId;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple connects at each special angle: the stop or its reflection, firing cancellation and the pose a release drops into.</summary>
internal static class GrappleConnections
{
    /// <summary>The native record at <paramref name="index"/>: an authored stop or the reflection that follows it.</summary>
    internal static SpecialConnection SpecialAngle(int index)
    {
        if ((uint)index >= SpecialAngleCount) throw new ArgumentOutOfRangeException(nameof(index));
        SpecialConnection stop = AuthoredStops[index >> 1];
        return (index & 1) == 0 ? stop : new(RotationAngle.FromRaw(unchecked((ushort)(ReflectionAngle - stop.Angle.RawValue))), SamusPoseMirror.Of(stop.Pose),
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
