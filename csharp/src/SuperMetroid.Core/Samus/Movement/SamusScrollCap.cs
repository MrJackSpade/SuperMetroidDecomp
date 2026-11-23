namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The scrolling-speed cap for enemies that place Samus (Draygon and Yapping Maw): it rewrites her
/// previous position so the camera follows at most one pixel past <see cref="SamusCameraDefinitions.PreviousPositionCap"/> a frame.
/// </summary>
internal sealed partial class SamusScrollCap
{
    /// <summary>
    /// The camera's frame-start Samus checkpoint for this enemy frame. The live previous-position
    /// position is this point plus the frame's writes so far. Null when no camera
    /// owns them.
    /// </summary>
    private SamusScrollCheckpoint _samusPreviousPositionCheckpoint = SamusScrollCheckpoint.SincePlacement;

    /// <summary>Enemy frame start: the camera's checkpoint of Samus's previous position, or none.</summary>
    internal void BeginFrame(SamusScrollCheckpoint samusPreviousPositionCheckpoint) =>
        _samusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint;

    /// <summary>
    /// The scrolling-speed cap, called by Draygon and Yapping Maw after they place
    /// Samus. An axis that moved at least <see cref="SamusCameraDefinitions.PreviousPositionCap"/> from its previous whole position gets a
    /// previous position that far away, so the camera follows at most a pixel more a frame.
    /// </summary>
    /// <remarks>
    /// The offset takes the movement's sign: a downward or rightward move puts the previous
    /// position beyond Samus, not behind her. Only the distance reaches the camera, whose
    /// direction comes from separate values. The cap's distance past Samus still lies in the room. Draygon moves her that
    /// far or more in an update only downward, as it grabs her, and around its spiral, which keeps her well inside the
    /// room's left edge and below its top. A Yapping Maw's mouth reaches no further than where she stood and holds her
    /// only a little beyond it, and every Maw stands far enough inside its room: the one nearest a room's edge, in Bug
    /// Sand Hole, stands well inside the left edge, and the wall there keeps her further still from it.
    /// </remarks>
    internal void CapSamusScrollingSpeed(SamusState samus)
    {
        // Only the camera reads this position; a camera placed since its last scroll holds none to cap.
        if (_samusPreviousPositionCheckpoint is not SamusScrolledCheckpoint { Point: var checkpoint })
            return;

        Position16<RoomPixels> previous = samus.PeekPreviousPosition(checkpoint);
        if (CappedPreviousPosition(samus.Position.Y.Whole, previous.Y) is { } cappedY)
            samus.WritePreviousYPosition(cappedY);
        if (CappedPreviousPosition(samus.Position.X.Whole, previous.X) is { } cappedX)
            samus.WritePreviousXPosition(cappedX);
    }

    /// <summary>The previous position capped to the camera's reach from Samus, or null when it lies within it.</summary>
    private static int? CappedPreviousPosition(int position, int previous)
    {
        int delta = position - previous;
        if (Math.Abs(delta) < SamusCameraDefinitions.PreviousPositionCap)
            return null;
        return position + int.CopySign(SamusCameraDefinitions.PreviousPositionCap, delta);
    }
}
