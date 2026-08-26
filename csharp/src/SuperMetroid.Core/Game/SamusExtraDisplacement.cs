namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared arithmetic for the four external-displacement WRAM words consumed by bank `$90`.
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
    /// Adds `$0B5C.$0B5A` to a signed movement displacement using native wrapping 32-bit
    /// arithmetic. `$90:90E2` does this after choosing up/down from the separate Y-direction
    /// word, so a strong external value may reverse the actual collision scan direction.
    /// </summary>
    public static int AddToVerticalSpeedDisplacement(
        SamusKinematicsState kinematics,
        int ownDisplacement)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        return unchecked(ownDisplacement + kinematics.ExtraYFixed);
    }

    /// <summary>
    /// Computes `$90:923F`, the no-speed vertical movement used by grounded families.
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
        ArgumentNullException.ThrowIfNull(kinematics);
        ArgumentNullException.ThrowIfNull(horizontalSpeed);

        int extra = kinematics.ExtraYFixed;
        if (extra != 0)
            return AddPositiveDownwardBias(extra);

        if (kinematics.PositionAdjustedBySlope)
            return 0x00010000;

        uint total = ((uint)horizontalSpeed.TotalSpeed << 16) |
            horizontalSpeed.TotalSubspeed;
        return unchecked((int)(total + 0x00010000u));
    }

    /// <summary>
    /// Computes `$90:9288`, used by jump-transition frames that accept external Y movement
    /// but deliberately skip ordinary gravity. Null means both producer words were zero and
    /// native returned without invoking collision at all.
    /// </summary>
    public static int? CalculateExtraOnlyVerticalDisplacement(
        SamusKinematicsState kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        int extra = kinematics.ExtraYFixed;
        return extra == 0 ? null : AddPositiveDownwardBias(extra);
    }

    private static int AddPositiveDownwardBias(int displacement) =>
        displacement < 0
            ? displacement
            : unchecked(displacement + 0x00010000);
}
