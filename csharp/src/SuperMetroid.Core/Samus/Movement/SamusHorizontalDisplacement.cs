using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One frame's signed 1/65536-pixel X displacement together with the collision movement direction
/// that its calculator stored: the leftward calculator writes left (0) and
/// the rightward one writes right (1).
/// </summary>
/// <remarks>
/// The two are independent. The movers choose a move right or
/// a move left from the displacement's sign (zero is not negative, so it moves
/// right), while their unconditional solid-enemy probe reads the stored direction.
/// </remarks>
public readonly record struct SamusHorizontalDisplacement(SignedSixteenSixteen Displacement, SamusCollisionDirection CollisionDirection)
{
    /// <summary>
    /// Displacement from a base speed plus extra X displacement by an already-decided direction: the
    /// leftward calculator or the rightward one.
    /// </summary>
    public static SamusHorizontalDisplacement Toward(bool left, SamusState samus, UnsignedSixteenSixteen baseSpeed)
    {
        SignedSixteenSixteen extraDisplacement = samus.Kinematics.ExtraDisplacement.X;
        return left
            ? new(samus.HorizontalSpeed.CalculateLeftDisplacement(baseSpeed, extraDisplacement), SamusCollisionDirection.Left)
            : new(samus.HorizontalSpeed.CalculateRightDisplacement(baseSpeed, extraDisplacement), SamusCollisionDirection.Right);
    }

    /// <summary>
    /// The game's calculator choice: acceleration modes zero and two follow the pose's
    /// X direction; any other value (normally mode one, turning) reverses it. The game
    /// compares rather than validating the mode, so every value is handled this way.
    /// </summary>
    public static SamusHorizontalDisplacement ForPoseDirection(SamusState samus, UnsignedSixteenSixteen baseSpeed)
    {
        SamusFacingDirection facing = SamusPoseMetadata.ReadFacingDirection(samus);
        bool reversePoseDirection = samus.HorizontalSpeed.AccelerationMode is not (SamusAccelerationMode.Accelerating or SamusAccelerationMode.Decelerating);
        return Toward(reversePoseDirection ? facing == SamusFacingDirection.Right : facing == SamusFacingDirection.Left, samus, baseSpeed);
    }
}
