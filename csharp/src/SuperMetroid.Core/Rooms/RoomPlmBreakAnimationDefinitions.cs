namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Shared $84:A345-A3DD breakup-draw identities for one-block, horizontal,
/// vertical and square parents. Programs select a frame; draw providers own
/// the complete terrain words and presentation independently.
/// </summary>
internal static class RoomPlmBreakAnimationDefinitions
{
    /// <summary>Selects a breakup draw program for one parent shape and animation frame.</summary>
    /// <param name="shape">Shape selector: 0 for one block, 1 horizontal, 2 vertical, or 3 square.</param>
    /// <param name="frame">Zero-based frame in the seven-step sequence; frames after the peak reuse earlier draws in reverse.</param>
    /// <returns>The native draw-program address for the selected shape and frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The shape is outside 0–3 or the frame is outside 0–6.</exception>
    internal static ushort DrawForShape(int shape, int frame)
    {
        (ushort first, int stride) = shape switch
        {
            0 => (RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6),
            1 => (RoomPlmShotBlockDrawDefinitions.HorizontalFrame0, 8),
            2 => (RoomPlmShotBlockDrawDefinitions.VerticalFrame0, 8),
            3 => (RoomPlmShotBlockDrawDefinitions.SquareFrame0, 16),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        if ((uint)frame >= 7) throw new ArgumentOutOfRangeException(nameof(frame));
        int ascendingFrame = frame < 4 ? frame : 6 - frame;
        return checked((ushort)(first + stride * ascendingFrame));
    }
}
