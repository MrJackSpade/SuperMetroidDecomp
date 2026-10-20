namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One frame's signed 16.16 X displacement together with the collision movement direction
/// ($0B02) that its native calculator stored: <c>$90:E464</c> writes left (0) and
/// <c>$90:E4AD</c> writes right (1).
/// </summary>
/// <remarks>
/// The two are independent. The bank-$90 movers choose <c>MoveSamus_Right</c> or
/// <c>MoveSamus_Left</c> from the displacement's sign (zero is not negative, so it moves
/// right), while their unconditional solid-enemy probe reads the stored direction.
/// </remarks>
public readonly record struct SamusHorizontalDisplacement(int Displacement, SamusCollisionDirection CollisionDirection)
{
    /// <summary>
    /// Displacement from a base speed plus extra X displacement by an already-decided direction: <c>$90:E464</c>'s
    /// leftward calculator or <c>$90:E4AD</c>'s rightward one.
    /// </summary>
    public static SamusHorizontalDisplacement Toward(bool left, SamusState samus, uint baseSpeed)
    {
        int extraDisplacement = samus.Kinematics.ExtraDisplacement.X.RawValue;
        return left
            ? new(samus.HorizontalSpeed.CalculateLeftDisplacement(baseSpeed, extraDisplacement), SamusCollisionDirection.Left)
            : new(samus.HorizontalSpeed.CalculateRightDisplacement(baseSpeed, extraDisplacement), SamusCollisionDirection.Right);
    }

    /// <summary>
    /// <c>$90:8EA9</c>'s calculator choice: acceleration modes zero and two follow the pose's
    /// X direction; any other word value (normally mode one, turning) reverses it. The 65C816
    /// compares rather than validating an enum, so every value is handled this way.
    /// </summary>
    public static SamusHorizontalDisplacement ForPoseDirection(SamusState samus, uint baseSpeed)
    {
        byte direction = samus.PoseMetadata.ReadPoseXDirection();
        bool reversePoseDirection = samus.HorizontalSpeed.AccelerationMode is not (0 or 2);
        return Toward(reversePoseDirection ? direction == 8 : direction == 4, samus, baseSpeed);
    }
}
