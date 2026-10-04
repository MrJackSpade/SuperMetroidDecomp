namespace SuperMetroid.Core.Game;

/// <summary>Calculated geometry and visibility timing for Wrecked Ship ghost appearances.</summary>
public static class WreckedShipGhostAppearanceDefinitions
{
    /// <summary>$A8:9AA8 contains the nine row-major positions of a three-by-three spawn grid.</summary>
    public const int SpawnCount = 9;

    /// <summary>$A8:9ACC contains sixteen alternating intervals followed by the $FFFF terminator.</summary>
    public const int FlickerCount = 17;

    /// <summary>
    /// Calculates the signed pair at $A8:9AA8 + 4*i, i=0..8. Horizontal and vertical
    /// movement classes select columns and rows, each spaced 64 pixels around Samus.
    /// The upper-right approach spawns level with Samus: its native Y word at $A8:9AB2
    /// is zero, consumed directly by the position addition at $A8:9DD1.
    /// </summary>
    public static (short X, short Y) SpawnOffset(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, SpawnCount);
        int horizontalDirection = index % 3 - 1;
        int verticalDirection = index / 3 - 1;
        bool approachingUpperRight = horizontalDirection > 0 && verticalDirection < 0;
        return ((short)(64 * horizontalDirection),
            (short)(approachingUpperRight ? 0 : 64 * verticalDirection));
    }

    /// <summary>
    /// Calculates $A8:9ACC + 2*i, i=0..16. Each four-interval stage repeats twice:
    /// the even interval grows from max(1, stage), the odd interval falls from 8-stage.
    /// Index16 terminates flickering. Native appearance starts at index1 but index0
    /// remains valid when the caller resets its offset after the terminator.
    /// </summary>
    public static short FlickerDuration(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, FlickerCount);
        if (index == FlickerCount - 1)
            return -1;
        int stage = index / 4;
        return (short)((index & 1) == 0 ? Math.Max(1, stage) : 8 - stage);
    }
}
