namespace SuperMetroid.Core.Game;

/// <summary>Fixed word window consumed by the translated fingernail contour walk.</summary>
internal static class KraidNailContour
{
    /// <summary>
    /// $A7:BF1D..BF2C: four authored left/top pairs. BF2D..BF34 contains the two
    /// adjacent instruction pairs that the native walk can also select. Exhaustive
    /// signed-word analysis proves every relative Y terminates by record five;
    /// no later instruction words or arbitrary host scan cap are needed.
    /// </summary>
    public static ReadOnlySpan<ushort> Words =>
    [
        0xffc0, 0x0010, 0xffd8, 0xffd8, 0xfff0, 0xffa0, 0x0008, 0xff80,
        0x0520, 0xadc0, 0x0fd2, 0x37c9,
    ];

    /// <summary>Returns the native selected left offset for a wrapped nail-minus-body Y word.</summary>
    public static ushort LeftOffset(ushort relativeY)
    {
        for (int index = 0; index < Words.Length; index += 2)
            if (unchecked((short)(Words[index + 1] - relativeY)) < 0)
                return Words[index];
        throw new InvalidOperationException("Kraid contour definitions failed their exhaustive signed-word coverage invariant.");
    }
}
