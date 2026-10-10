namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// The camera's frame-start Samus checkpoint for this enemy frame. The live previous-position
    /// words ($0B10/$0B14) are this point plus the frame's writes so far. Null when no camera
    /// owns them, as in standalone enemy fixtures.
    /// </summary>
    private SamusCameraPoint? _samusPreviousPositionCheckpoint;

    /// <summary>
    /// <c>CapScrollingSpeed</c> ($A0:B7A1), called by Draygon and Yapping Maw after they place
    /// Samus. An axis that moved twelve or more pixels from its previous whole position gets a
    /// previous position twelve pixels away, so the camera follows at most thirteen a frame.
    /// </summary>
    /// <remarks>
    /// The offset takes the movement's sign: a downward or rightward move puts the previous
    /// position beyond Samus, not behind her. Only the distance reaches the camera, whose
    /// direction comes from separate words. A magnitude of $8000 stays negative after
    /// <c>NegateA_A0B067</c> but compares as not below twelve, so it is capped too.
    /// </remarks>
    private void CapSamusScrollingSpeed(SamusState samus)
    {
        // Only the camera reads these words; without one there is nothing to cap.
        if (_samusPreviousPositionCheckpoint is not { } checkpoint)
            return;

        (ushort previousX, ushort previousY) = samus.PeekPreviousPositionWords(checkpoint);
        if (CappedPreviousPosition(samus.YPosition, previousY) is { } cappedY)
            samus.WritePreviousYPosition(cappedY);
        if (CappedPreviousPosition(samus.XPosition, previousX) is { } cappedX)
            samus.WritePreviousXPosition(cappedX);
    }

    /// <summary>Clamps a camera checkpoint to twelve pixels from the current coordinate when movement reaches the cap.</summary>
    /// <param name="position">Current Samus coordinate on one scrolling axis.</param>
    /// <param name="previous">Frame-start checkpoint coordinate for the same axis.</param>
    /// <returns>The wrapped checkpoint twelve pixels beyond the current coordinate in the movement direction, or <see langword="null"/> below the cap.</returns>
    private static ushort? CappedPreviousPosition(ushort position, ushort previous)
    {
        ushort delta = unchecked((ushort)(position - previous));
        ushort magnitude = (delta & 0x8000) != 0 ? unchecked((ushort)-delta) : delta;
        // CMP #$000C / BMI: the sign of magnitude - 12.
        if (unchecked((short)(magnitude - 0x000c)) < 0)
            return null;
        short offset = (delta & 0x8000) != 0 ? (short)-0x0c : (short)0x0c;
        return unchecked((ushort)(position + offset));
    }
}
