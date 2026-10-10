namespace SuperMetroid.Core.Game;

/// <summary>Norfair Ridley's facing as the arena-target tables index it.</summary>
public enum RidleyFacing : ushort
{
    /// <summary>Native facing index zero, left-facing Ridley.</summary>
    Left = 0,
    /// <summary>Native facing index one, intermediate turning pose.</summary>
    Turning = 1,
    /// <summary>Native facing index two, right-facing Ridley.</summary>
    Right = 2,
}

/// <summary>Arena targets and health-dependent inertia selectors for Norfair Ridley.</summary>
public static class RidleyMovementTargets
{
    /// <summary>Selects the target-table facing from Ridley's facing word; words past two select the right-facing entry.</summary>
    public static RidleyFacing Facing(ushort facingWord) =>
        ClosedNativeWords.Decode<RidleyFacing>(Math.Min(facingWord, (ushort)RidleyFacing.Right), "Ridley facing");

    /// <summary>$A6:B60D Ridley_Func_15: descending pogo uses quarter-arena points in reverse facing order.</summary>
    public static ushort DescendingPogoX(RidleyFacing facing) => (ushort)(256 - AscendingPogoX(facing));

    /// <summary>$A6:B63B Ridley_Func_16: ascending pogo uses successive 64-pixel quarter-arena points.</summary>
    public static ushort AscendingPogoX(RidleyFacing facing)
    {
        ValidateFacing(facing);
        return (ushort)(64 * ((int)facing + 1));
    }

    /// <summary>$A6:B6C8 Ridley_Func_19: left, turning and right attack staging positions.</summary>
    public static ushort GroundAttackX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 176,
        RidleyFacing.Turning => 128,
        RidleyFacing.Right => 96,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {(int)facing}."),
    };

    /// <summary>$A6:BBEB Ridley_Func_36: carry anchors at the left/right sides; turning has no anchor.</summary>
    public static ushort CarryAnchorX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 64,
        RidleyFacing.Turning => 0,
        RidleyFacing.Right => 208,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {(int)facing}."),
    };

    /// <summary>$A6:BC62: carry-release destinations reflected about the arena center; turning has no target.</summary>
    public static ushort CarryReleaseX(RidleyFacing facing)
    {
        ValidateFacing(facing);
        return facing == RidleyFacing.Turning ? (ushort)0 : (ushort)(176 - 48 * (int)facing);
    }

    /// <summary>$A6:B439: hover divisor index first rises by four, then by two per health stage.</summary>
    public static ushort HoverDivisorIndexes(int stage)
    {
        ValidateStage(stage);
        return (ushort)(4 + 2 * stage + 2 * Math.Min(stage, 1));
    }

    /// <summary>$A6:BB4E: grab divisor indexes double-plus-one until the final cap of ten.</summary>
    public static ushort GrabDivisorIndexes(int stage)
    {
        ValidateStage(stage);
        return (ushort)Math.Min(10, (1 << (stage + 1)) - 1);
    }

    private static void ValidateFacing(RidleyFacing facing)
    {
        if (!Enum.IsDefined(facing))
            throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {(int)facing}.");
    }

    private static void ValidateStage(int stage)
    {
        if ((uint)stage >= 4) throw new IndexOutOfRangeException();
    }
}
