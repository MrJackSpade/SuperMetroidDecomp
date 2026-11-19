using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One frame's X displacement together with the side it was calculated toward: a displacement calculated leftward
/// probes <see cref="SamusCollisionDirection.Left"/> and one calculated rightward <see cref="SamusCollisionDirection.Right"/>.
/// </summary>
/// <remarks>
/// The two are independent. The movers choose a move right or
/// a move left from the displacement's sign (zero is not negative, so it moves
/// right), while their unconditional solid-enemy probe reads the side it was calculated toward.
/// </remarks>
public readonly record struct SamusHorizontalDisplacement(SignedSixteenSixteen Displacement, SamusCollisionDirection CollisionDirection)
{
    /// <summary>
    /// Displacement from a base speed plus the extra X displacement, calculated toward the side already decided: left
    /// when <paramref name="left"/>, otherwise right.
    /// </summary>
    public static SamusHorizontalDisplacement Toward(bool left, SamusState samus, UnsignedSixteenSixteen baseSpeed)
    {
        SignedSixteenSixteen extraDisplacement = samus.Kinematics.ExtraDisplacement.X;
        return left
            ? new(samus.HorizontalSpeed.CalculateLeftDisplacement(baseSpeed, extraDisplacement), SamusCollisionDirection.Left)
            : new(samus.HorizontalSpeed.CalculateRightDisplacement(baseSpeed, extraDisplacement), SamusCollisionDirection.Right);
    }

    /// <summary>
    /// Displacement calculated toward the side her acceleration mode takes: accelerating and decelerating follow the
    /// pose's facing; turning runs against it.
    /// </summary>
    public static SamusHorizontalDisplacement ForPoseDirection(SamusState samus, UnsignedSixteenSixteen baseSpeed)
    {
        SamusFacingDirection facing = SamusPoseMetadata.ReadFacingDirection(samus);
        bool reversePoseDirection = samus.HorizontalSpeed.AccelerationMode is not (SamusAccelerationMode.Accelerating or SamusAccelerationMode.Decelerating);
        return Toward(reversePoseDirection ? facing == SamusFacingDirection.Right : facing == SamusFacingDirection.Left, samus, baseSpeed);
    }
}
