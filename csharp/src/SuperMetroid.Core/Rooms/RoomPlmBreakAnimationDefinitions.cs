namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Shared $84:A345-A3DD breakup-draw identities for one-block, horizontal,
/// vertical and square parents. Programs select a frame; draw providers own
/// the complete terrain words and presentation independently.
/// </summary>
internal static class RoomPlmBreakAnimationDefinitions
{
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
