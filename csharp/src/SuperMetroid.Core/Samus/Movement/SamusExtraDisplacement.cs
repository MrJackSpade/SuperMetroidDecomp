namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Shared arithmetic for the four external-displacement words consumed by Samus's movement.
/// </summary>
/// <remarks>
/// These values are not velocity and are not one-frame impulses. Enemy, PLM, and room
/// producers own their lifetime; Samus movement merely reads them. Keeping the arithmetic
/// here prevents a convenient but incorrect host interpretation such as clearing them after
/// movement or applying the grounded one-pixel bias to airborne gravity.
/// </remarks>
internal static class SamusExtraDisplacement
{
    /// <summary>
    /// Adds the external Y displacement to a signed movement displacement using wrapping 32-bit
    /// arithmetic. The vertical move does this after choosing up/down from the separate Y-direction
    /// word, so a strong external value may reverse the actual collision scan direction.
    /// </summary>
    public static int AddToVerticalSpeedDisplacement(
        SamusKinematicsState kinematics,
        int ownDisplacement)
    {
        Ensure.NotNull(kinematics);
        return unchecked(ownDisplacement + kinematics.ExtraDisplacement.Y.RawValue);
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
    public static int CalculateNoSpeedVerticalDisplacement(
        SamusKinematicsState kinematics,
        SamusHorizontalSpeedState horizontalSpeed)
    {
        Ensure.NotNull(kinematics);
        Ensure.NotNull(horizontalSpeed);

        int extra = kinematics.ExtraDisplacement.Y.RawValue;
        if (extra != 0)
            return AddPositiveDownwardBias(extra);

        if (kinematics.PositionAdjustedBySlope)
            return 1 << 16;

        uint total = (SixteenSixteen.FromWhole((uint)horizontalSpeed.TotalSpeed.Whole)) |
            (uint)horizontalSpeed.TotalSpeed.Fraction;
        return unchecked((int)(total + (1u << 16)));
    }

    /// <summary>
    /// Computes the transition vertical movement, used by jump-transition frames that accept external Y movement
    /// but deliberately skip ordinary gravity. Null means both producer words were zero and
    /// the game returned without invoking collision at all.
    /// </summary>
    public static int? CalculateExtraOnlyVerticalDisplacement(
        SamusKinematicsState kinematics)
    {
        Ensure.NotNull(kinematics);
        int extra = kinematics.ExtraDisplacement.Y.RawValue;
        return extra == 0 ? null : AddPositiveDownwardBias(extra);
    }

    /// <summary>Adds one whole pixel only to nonnegative external Y displacement before the grounded or extra-only collision scan.</summary>
    private static int AddPositiveDownwardBias(int displacement) =>
        displacement < 0
            ? displacement
            : unchecked(displacement + (1 << 16));
}
