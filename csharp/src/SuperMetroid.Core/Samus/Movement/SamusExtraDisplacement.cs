namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Shared arithmetic for the external displacement Samus's movement reads (<see cref="SamusKinematicsState.ExtraDisplacement"/>).
/// </summary>
/// <remarks>
/// The displacement is not velocity and is not a one-frame impulse. Its enemy, room-object and room
/// producers own its lifetime; Samus movement merely reads it. Keeping the arithmetic
/// here prevents a convenient but incorrect reading such as clearing it after
/// movement or applying the grounded one-pixel bias to airborne gravity.
/// </remarks>
internal static class SamusExtraDisplacement
{
    /// <summary>
    /// Adds the external vertical displacement to Samus's own vertical displacement, up or down. The vertical move does this after choosing up or down from her vertical direction, so a strong external value may reverse the actual collision scan direction.
    /// </summary>
    public static SignedSixteenSixteen AddToVerticalSpeedDisplacement(
        SamusKinematicsState kinematics,
        SignedSixteenSixteen ownDisplacement)
    {
        Ensure.NotNull(kinematics);
        return ownDisplacement + kinematics.ExtraDisplacement.Y;
    }

    /// <summary>
    /// Computes the no-speed vertical movement used by grounded families.
    /// </summary>
    /// <remarks>
    /// A nonzero external value completely replaces the slope-following probe. Positive
    /// values receive one extra whole pixel before moving down; negative values move up
    /// exactly as written. With no external value, total X speed plus one keeps Samus on a
    /// descending slope unless the preceding X scan already aligned her Y position.
    /// </remarks>
    public static SignedSixteenSixteen CalculateNoSpeedVerticalDisplacement(
        SamusKinematicsState kinematics,
        SamusHorizontalSpeedState horizontalSpeed)
    {
        Ensure.NotNull(kinematics);
        Ensure.NotNull(horizontalSpeed);

        SignedSixteenSixteen extra = kinematics.ExtraDisplacement.Y;
        if (!extra.IsZero)
            return AddPositiveDownwardBias(extra);

        if (kinematics.PositionAdjustedBySlope)
            return OnePixel;

        return horizontalSpeed.TotalSpeed.AsDisplacement() + OnePixel;
    }

    /// <summary>One whole pixel downward: the grounded bias and the slope-adjusted probe.</summary>
    private static readonly SignedSixteenSixteen OnePixel = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>
    /// Computes the transition vertical movement, used by jump-transition frames that accept external Y movement
    /// but deliberately skip ordinary gravity. Null means the external displacement has no vertical part, and
    /// the frame neither moves her vertically nor scans for collision.
    /// </summary>
    public static SignedSixteenSixteen? CalculateExtraOnlyVerticalDisplacement(
        SamusKinematicsState kinematics)
    {
        Ensure.NotNull(kinematics);
        SignedSixteenSixteen extra = kinematics.ExtraDisplacement.Y;
        return extra.IsZero ? null : AddPositiveDownwardBias(extra);
    }

    /// <summary>Adds one whole pixel only to nonnegative external Y displacement before the grounded or extra-only collision scan.</summary>
    internal static SignedSixteenSixteen AddPositiveDownwardBias(SignedSixteenSixteen displacement) =>
        displacement.IsNegative
            ? displacement
            : displacement + OnePixel;
}
