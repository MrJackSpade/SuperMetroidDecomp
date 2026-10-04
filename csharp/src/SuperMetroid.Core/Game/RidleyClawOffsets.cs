namespace SuperMetroid.Core.Game;

/// <summary>Signed claw displacements used by Ridley's grab collision and carried-Samus placement.</summary>
public static class RidleyClawOffsets
{
    /// <summary>
    /// $A6:B9E1..B9EB, adjacent MoveSamusWithRidleyFeet instruction bytes as six
    /// little-endian words. These are NOT authored offsets. Retain the existing
    /// host's clamped read window for non-authored state values during migration;
    /// ordinary foot animation indexes only Y. This is not a native bounds rule.
    /// </summary>
    private static ReadOnlySpan<short> ExistingOutOfRangeWindow =>
        [0x28af, 0x7e78, 0x1ff0, 0x1285, 0x0410, -183];

    /// <summary>$A6:B9D5 HoldingSamusXDispacement: left/turning/right displacements decrease twelve pixels per facing; preserves the host clamp.</summary>
    public static short ReadX(ushort facing) => (short)(12 - 12 * Math.Min(facing, (ushort)2));

    /// <summary>$A6:B9DB HoldingSamusYDispacement: three foot heights advance 10.5 pixels rounded upward; preserves word indexing and the adjacent host window.</summary>
    public static short ReadY(ushort feetDistanceIndex)
    {
        int index = Math.Min(feetDistanceIndex >> 1, 8);
        return index < 3 ? (short)(35 + (21 * index + 1) / 2) : ExistingOutOfRangeWindow[index - 3];
    }
}
