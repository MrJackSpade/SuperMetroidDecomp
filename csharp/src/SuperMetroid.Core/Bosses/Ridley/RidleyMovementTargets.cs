namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Arena targets and health-dependent inertia selectors for Norfair Ridley.</summary>
public static class RidleyMovementTargets
{
    /// <summary>Descending pogo uses quarter-arena points in reverse facing order.</summary>
    public static int DescendingPogoX(RidleyFacing facing) => 256 - AscendingPogoX(facing);

    /// <summary>Ascending pogo uses successive 64-pixel quarter-arena points.</summary>
    public static int AscendingPogoX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 64,
        RidleyFacing.Turning => 128,
        RidleyFacing.Right => 192,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>Left, turning and right attack staging positions.</summary>
    public static int GroundAttackX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 176,
        RidleyFacing.Turning => 128,
        RidleyFacing.Right => 96,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>Carry anchors at the left/right sides; turning has no anchor.</summary>
    public static int CarryAnchorX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 64,
        RidleyFacing.Turning => 0,
        RidleyFacing.Right => 208,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>Carry-release destinations reflected about the arena center; turning has no target.</summary>
    public static int CarryReleaseX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 176,
        RidleyFacing.Turning => 0,
        RidleyFacing.Right => 80,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>The hover divisor: twelve at full health, falling by four at the first stage and by two at each later one.</summary>
    public static int HoverDivisor(int stage)
    {
        ValidateStage(stage);
        return RidleyInertiaDefinitions.LargestDivisor - (4 + 2 * stage + 2 * Math.Min(stage, 1));
    }

    /// <summary>The grab divisor: fifteen at full health, then 13 and 9, falling no lower than six.</summary>
    public static int GrabDivisor(int stage)
    {
        ValidateStage(stage);
        return RidleyInertiaDefinitions.LargestDivisor - Math.Min(10, (1 << (stage + 1)) - 1);
    }

    /// <summary>Limits health-stage selection to the four entries represented by the movement tables.</summary>
    private static void ValidateStage(int stage)
    {
        if ((uint)stage >= 4) throw new ArgumentOutOfRangeException(nameof(stage));
    }
}
