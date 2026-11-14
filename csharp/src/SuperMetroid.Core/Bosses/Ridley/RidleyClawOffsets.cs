namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Signed claw displacements used by Ridley's grab collision and carried-Samus placement.</summary>
public static class RidleyClawOffsets
{
    /// <summary>The held Samus's horizontal offset: left/turning/right displacements decrease twelve pixels per facing.</summary>
    public static int ReadX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 12,
        RidleyFacing.Turning => 0,
        RidleyFacing.Right => -12,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>
    /// The held Samus's vertical offset: the claw's height below Ridley's origin for <paramref name="height"/>, lower by
    /// 10.5 pixels, rounded upward, for each step his legs extend.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="height"/> is not one of the three claw heights.</exception>
    public static int ReadY(RidleyClawHeight height) => height switch
    {
        RidleyClawHeight.Standing => 35,
        RidleyClawHeight.LegsHalfExtended => 46,
        RidleyClawHeight.LegsExtended => 56,
        _ => throw new ArgumentOutOfRangeException(nameof(height), height, "Ridley has three claw heights."),
    };
}
