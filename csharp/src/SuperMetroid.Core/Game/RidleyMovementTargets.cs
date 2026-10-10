namespace SuperMetroid.Core.Game;

/// <summary>Arena targets and health-dependent inertia selectors for Norfair Ridley.</summary>
public static class RidleyMovementTargets
{
    /// <summary>Native facing index zero, left-facing Ridley.</summary>
    private const int Left = 0;
    /// <summary>Native facing index one, intermediate turning pose.</summary>
    private const int Turning = 1;
    /// <summary>Native facing index two, right-facing Ridley.</summary>
    private const int Right = 2;

    /// <summary>$A6:B60D Ridley_Func_15: descending pogo uses quarter-arena points in reverse facing order.</summary>
    public static ushort DescendingPogoX(int facing) => (ushort)(256 - AscendingPogoX(facing));

    /// <summary>$A6:B63B Ridley_Func_16: ascending pogo uses successive 64-pixel quarter-arena points.</summary>
    public static ushort AscendingPogoX(int facing)
    {
        ValidateFacing(facing);
        return (ushort)(64 * (facing + 1));
    }

    /// <summary>$A6:B6C8 Ridley_Func_19: left, turning and right attack staging positions.</summary>
    public static ushort GroundAttackX(int facing) => facing switch
    {
        Left => 176,
        Turning => 128,
        Right => 96,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$A6:BBEB Ridley_Func_36: carry anchors at the left/right sides; turning has no anchor.</summary>
    public static ushort CarryAnchorX(int facing) => facing switch
    {
        Left => 64,
        Turning => 0,
        Right => 208,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$A6:BC62: carry-release destinations reflected about the arena center; turning has no target.</summary>
    public static ushort CarryReleaseX(int facing)
    {
        ValidateFacing(facing);
        return facing == Turning ? (ushort)0 : (ushort)(176 - 48 * facing);
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

    /// <summary>Ensures a movement-target lookup uses one of the native left, turning, or right facing indexes.</summary>
    /// <param name="facing">Facing index, which must be between zero and two inclusive.</param>
    /// <exception cref="IndexOutOfRangeException">The facing index is outside the three supported orientations.</exception>
    private static void ValidateFacing(int facing)
    {
        if ((uint)facing > Right) throw new IndexOutOfRangeException();
    }

    /// <summary>Ensures a health-dependent divisor lookup uses one of Ridley's four stages.</summary>
    /// <param name="stage">Zero-based health stage, from zero through three.</param>
    /// <exception cref="IndexOutOfRangeException">The stage is outside the four supported values.</exception>
    private static void ValidateStage(int stage)
    {
        if ((uint)stage >= 4) throw new IndexOutOfRangeException();
    }
}
