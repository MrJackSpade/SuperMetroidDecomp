namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed timing definitions for Draygon's opening Evir dance.</summary>
internal static class DraygonIntroDanceDefinitions
{
    /// <summary>
    /// The four reachable words of <c>MovementLatencyForEachEvirSpriteObject</c> at
    /// <c>$A5:A19F-$A5:A1A6</c>, ordered by sprite slots 28 through 31.
    /// </summary>
    private static ReadOnlySpan<short> MovementLatencies =>
        [-0x0380, -0x0300, -0x0280, -0x0200];

    /// <summary>Native byte-index advance after each dance update at <c>$A5:A188</c>.</summary>
    public const ushort StreamIndexAdvance = 4;

    /// <summary>Native dance duration checked at <c>$A5:8790</c>.</summary>
    public const ushort DurationFrames = 0x04d0;

    /// <summary>Returns the signed stream latency for native sprite slot 28 through 31.</summary>
    public static short MovementLatencyForSlot(int slotIndex)
    {
        int index = slotIndex - 28;
        if ((uint)index >= MovementLatencies.Length)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        return MovementLatencies[index];
    }
}
