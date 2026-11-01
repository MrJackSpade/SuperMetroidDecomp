namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Arena targets and health-dependent inertia selectors for Norfair Ridley.</summary>
public static class RidleyMovementTargets
{
    /// <summary>Descending pogo uses quarter-arena points in reverse facing order.</summary>
    public static int DescendingPogoX(RidleyFacing facing) => WordWrap.Unsigned(256 - AscendingPogoX(facing));

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

    /// <summary>Hover divisor index first rises by four, then by two per health stage.</summary>
    public static int HoverDivisorIndexes(int stage)
    {
        ValidateStage(stage);
        return WordWrap.Unsigned(4 + 2 * stage + 2 * Math.Min(stage, 1));
    }

    /// <summary>Grab divisor indexes double-plus-one until the final cap of ten.</summary>
    public static int GrabDivisorIndexes(int stage)
    {
        ValidateStage(stage);
        return WordWrap.Unsigned(Math.Min(10, (1 << (stage + 1)) - 1));
    }

    /// <summary>Limits health-stage selection to the four entries represented by the movement tables.</summary>
    private static void ValidateStage(int stage)
    {
        if ((uint)stage >= 4) throw new ArgumentOutOfRangeException(nameof(stage));
    }
}
