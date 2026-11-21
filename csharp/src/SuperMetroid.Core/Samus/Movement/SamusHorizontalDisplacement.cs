using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One frame's X displacement together with the side it was calculated toward: a displacement calculated leftward
/// probes left and one calculated rightward probes right.
/// </summary>
/// <remarks>
/// The two are independent. The movers choose a move right or
/// a move left from the displacement's sign (zero is not negative, so it moves
/// right), while their unconditional solid-enemy probe reads the side it was calculated toward.
/// </remarks>
public readonly partial record struct SamusHorizontalDisplacement(SignedSixteenSixteen Displacement, HorizontalDirection CollisionDirection)
{
    /// <summary>
    /// Displacement from a base speed plus the extra X displacement, calculated toward <paramref name="side"/>, the side
    /// already decided (#2569).
    /// </summary>
    public static SamusHorizontalDisplacement Toward(HorizontalDirection side, SamusState samus, UnsignedSixteenSixteen baseSpeed) => new(
        samus.HorizontalSpeed.CalculateDisplacement(side, baseSpeed, samus.Kinematics.ExtraDisplacement.X),
        side);

    /// <summary>
    /// Displacement calculated toward the side her pose faces (#2569): left for a left-facing pose, otherwise right, a front
    /// view included.
    /// </summary>
    public static SamusHorizontalDisplacement TowardFacing(SamusState samus, UnsignedSixteenSixteen baseSpeed) => Toward(
        SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? HorizontalDirection.Left : HorizontalDirection.Right, samus, baseSpeed);

    /// <summary>
    /// Displacement calculated toward the side her acceleration mode takes: accelerating and decelerating follow the
    /// pose's facing; turning runs against it.
    /// </summary>
    public static SamusHorizontalDisplacement ForPoseDirection(SamusState samus, UnsignedSixteenSixteen baseSpeed)
    {
        SamusFacingDirection facing = SamusPoseMetadata.ReadFacingDirection(samus);
        bool reversePoseDirection = samus.HorizontalSpeed.AccelerationMode is not (SamusAccelerationMode.Accelerating or SamusAccelerationMode.Decelerating);
        HorizontalDirection side = facing == (reversePoseDirection ? SamusFacingDirection.Right : SamusFacingDirection.Left)
            ? HorizontalDirection.Left : HorizontalDirection.Right;
        return Toward(side, samus, baseSpeed);
    }
}
