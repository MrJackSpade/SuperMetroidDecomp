namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's arena targets by the way he faces.</summary>
public static class RidleyMovementTargets
{
    /// <summary>The wall sweep's point behind Ridley: the quarter-arena point opposite <see cref="WallAheadX"/>.</summary>
    public static int WallBehindX(RidleyFacing facing) => 256 - WallAheadX(facing);

    /// <summary>The wall sweep's point ahead of Ridley: successive 64-pixel quarter-arena points by facing.</summary>
    public static int WallAheadX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 64,
        RidleyFacing.Turning => 128,
        RidleyFacing.Right => 192,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>The side of the arena the pogo attack starts from, by facing.</summary>
    public static int PogoSideX(RidleyFacing facing) => facing switch
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
}
