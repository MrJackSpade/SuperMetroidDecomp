namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Native <c>CapScrollingSpeed</c> for enemies that place Samus (Draygon and Yapping Maw): it rewrites her
/// previous-position words so the camera follows at most thirteen pixels a frame.
/// </summary>
internal sealed class SamusScrollCap
{
    /// <summary>
    /// The camera's frame-start Samus checkpoint for this enemy frame. The live previous-position
    /// words ($0B10/$0B14) are this point plus the frame's writes so far. Null when no camera
    /// owns them, as in standalone enemy fixtures.
    /// </summary>
    internal FixedPosition<RoomPixels>? _samusPreviousPositionCheckpoint;

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
    internal void CapSamusScrollingSpeed(SamusState samus)
    {
        // Only the camera reads these words; without one there is nothing to cap.
        if (_samusPreviousPositionCheckpoint is not { } checkpoint)
            return;

        (ushort previousX, ushort previousY) = samus.PeekPreviousPositionWords(checkpoint);
        if (CappedPreviousPosition(samus.Position.Y.Whole, previousY) is { } cappedY)
            samus.WritePreviousYPosition(cappedY);
        if (CappedPreviousPosition(samus.Position.X.Whole, previousX) is { } cappedX)
            samus.WritePreviousXPosition(cappedX);
    }

    /// <summary>Returns a previous-position word twelve pixels from Samus when native signed-distance comparison reaches the camera cap.</summary>
    private static ushort? CappedPreviousPosition(ushort position, ushort previous)
    {
        ushort delta = unchecked((ushort)(position - previous));
        ushort magnitude = unchecked((short)delta) < 0 ? unchecked((ushort)-delta) : delta;
        // CMP #$000C / BMI: the sign of magnitude - 12.
        if (unchecked((short)(magnitude - 12)) < 0)
            return null;
        short offset = unchecked((short)delta) < 0 ? (short)-12 : (short)12;
        return unchecked((ushort)(position + offset));
    }
}
