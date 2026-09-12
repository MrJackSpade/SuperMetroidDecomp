namespace SuperMetroid.Core.Game;

/// <summary>Signed claw displacements used by Ridley's grab collision and carried-Samus placement.</summary>
public static class RidleyClawOffsets
{
    /// <summary>$A6:B9D5, HoldingSamusXDispacement: left, turning and right-facing offsets.</summary>
    public static ReadOnlySpan<short> X => [12, 0, -12];

    /// <summary>$A6:B9DB, HoldingSamusYDispacement: three authored foot-animation offsets.</summary>
    public static ReadOnlySpan<short> Y => [35, 46, 56];

    /// <summary>
    /// $A6:B9E1..B9EB, adjacent MoveSamusWithRidleyFeet instruction bytes as six
    /// little-endian words. These are NOT authored offsets. Retain the existing
    /// host's clamped read window for non-authored state values during migration;
    /// ordinary foot animation indexes only Y. This is not a native bounds rule.
    /// </summary>
    private static ReadOnlySpan<short> ExistingOutOfRangeWindow =>
        [0x28af, 0x7e78, 0x1ff0, 0x1285, 0x0410, -183];

    /// <summary>Preserves the translated facing clamp while replacing its immutable ROM read.</summary>
    public static short ReadX(ushort facing) => X[Math.Min(facing, (ushort)2)];

    /// <summary>Preserves word-index conversion and the existing nine-word read window.</summary>
    public static short ReadY(ushort feetDistanceIndex)
    {
        int index = Math.Min(feetDistanceIndex >> 1, 8);
        return index < Y.Length ? Y[index] : ExistingOutOfRangeWindow[index - Y.Length];
    }
}
