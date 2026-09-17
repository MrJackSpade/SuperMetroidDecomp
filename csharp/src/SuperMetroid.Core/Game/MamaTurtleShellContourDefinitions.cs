namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled physical contour of Mama Turtle's sleeping shell. The first half describes
/// actors left of the parent origin; the second half describes actors to its right.
/// </summary>
internal static class MamaTurtleShellContourDefinitions
{
    /// <summary>First signed contour word at <c>$A2:8E80</c>.</summary>
    internal const int SourceAddress = 0xa28e80;

    /// <summary>Number of signed words in the two 24-pixel contour halves.</summary>
    internal const int EntryCount = 48;

    /// <summary>Maximum horizontal distance represented by either contour half.</summary>
    internal const int HalfWidth = 24;

    private static ReadOnlySpan<short> Offsets =>
    [
        -16, -16, -16, -16, -15, -15, -15, -15,
        -15, -14, -13, -13, -12, -11, -10, -9,
        -8, -7, -6, -5, -4, -4, 0, 0,
        -16, -16, -16, -15, -15, -15, -14, -13,
        -12, -11, -10, -9, -8, -7, -6, -5,
        -4, -3, -3, -2, 0, 0, 0, 0,
    ];

    /// <summary>
    /// Returns the native signed vertical contour offset for a parent-minus-actor X
    /// difference whose magnitude is below twenty-four pixels.
    /// </summary>
    internal static short GetOffset(short parentMinusActorX)
    {
        int distance = Math.Abs((int)parentMinusActorX);
        if (distance >= HalfWidth)
            throw new ArgumentOutOfRangeException(nameof(parentMinusActorX));
        int index = parentMinusActorX < 0 ? distance + HalfWidth : distance;
        return Offsets[index];
    }

    /// <summary>Returns one raw word by native table index for cartridge parity checks.</summary>
    internal static short GetRawOffset(int index)
    {
        if ((uint)index >= EntryCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return Offsets[index];
    }
}
