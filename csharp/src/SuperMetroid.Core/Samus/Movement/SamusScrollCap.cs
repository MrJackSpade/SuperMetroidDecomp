namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The scrolling-speed cap for enemies that place Samus (Draygon and Yapping Maw): it rewrites her
/// previous position so the camera follows at most thirteen pixels a frame.
/// </summary>
internal sealed class SamusScrollCap
{
    /// <summary>
    /// The camera's frame-start Samus checkpoint for this enemy frame. The live previous-position
    /// position is this point plus the frame's writes so far. Null when no camera
    /// owns them, as in standalone enemy fixtures.
    /// </summary>
    private FixedPosition<RoomPixels>? _samusPreviousPositionCheckpoint;

    /// <summary>Enemy frame start: the camera's checkpoint of Samus's previous position, or none.</summary>
    internal void BeginFrame(FixedPosition<RoomPixels>? samusPreviousPositionCheckpoint) =>
        _samusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint;

    /// <summary>
    /// The scrolling-speed cap, called by Draygon and Yapping Maw after they place
    /// Samus. An axis that moved twelve or more pixels from its previous whole position gets a
    /// previous position twelve pixels away, so the camera follows at most thirteen a frame.
    /// </summary>
    /// <remarks>
    /// The offset takes the movement's sign: a downward or rightward move puts the previous
    /// position beyond Samus, not behind her. Only the distance reaches the camera, whose
    /// direction comes from separate values. A magnitude of 32768 stays negative after
    /// negation but compares as not below twelve, so it is capped too.
    /// </remarks>
    internal void CapSamusScrollingSpeed(SamusState samus)
    {
        // Only the camera reads this position; without one there is nothing to cap.
        if (_samusPreviousPositionCheckpoint is not { } checkpoint)
            return;

        Position16<RoomPixels> previous = samus.PeekPreviousPosition(checkpoint);
        if (CappedPreviousPosition(samus.Position.Y.Whole, previous.Y) is { } cappedY)
            samus.WritePreviousYPosition(cappedY);
        if (CappedPreviousPosition(samus.Position.X.Whole, previous.X) is { } cappedX)
            samus.WritePreviousXPosition(cappedX);
    }

    /// <summary>Returns a previous position twelve pixels from Samus when the signed-distance comparison reaches the camera cap.</summary>
    private static int? CappedPreviousPosition(int position, int previous)
    {
        int delta = CoordinateAxis.Difference(position, previous);
        if (Math.Abs(delta) < 12)
            return null;
        return CoordinateAxis.Wrap(position + (delta < 0 ? -12 : 12));
    }
}
