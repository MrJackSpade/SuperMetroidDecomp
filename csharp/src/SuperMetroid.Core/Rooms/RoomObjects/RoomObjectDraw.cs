namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One draw a room object makes on the room's level data, identified by what it draws: its family's frame (#1402). Programs hand
/// draws to <see cref="IRoomObjectDrawing"/>, which dispatches on the frame's type; a family's visual catalog keys its authored
/// blocks by the draw. Where the game stored each draw is known only to the import pipeline.
/// </summary>
internal abstract record RoomObjectDraw
{
    /// <summary>The draw of <paramref name="frame"/>, one frame of a family's draws.</summary>
    /// <typeparam name="TFrame">The family's frame: an enum of its frames, or a record of what varies between them.</typeparam>
    internal static RoomObjectFrameDraw<TFrame> Of<TFrame>(TFrame frame) where TFrame : struct => new(frame);

    /// <summary>The frame of <paramref name="draw"/>, which must be one of <typeparamref name="TFrame"/>'s family.</summary>
    /// <param name="draw">The draw.</param>
    /// <param name="family">The family's name, for the rejection.</param>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> belongs to another family.</exception>
    internal static TFrame FrameOf<TFrame>(RoomObjectDraw draw, string family) where TFrame : struct =>
        draw is RoomObjectFrameDraw<TFrame> { Frame: var frame } ? frame : throw new InvalidDataException($"{draw} is not a {family} draw.");

    /// <summary>
    /// Whether <paramref name="draw"/> is one of <typeparamref name="TFrame"/>'s family whose frame <paramref name="accepts"/>
    /// admits; <paramref name="frame"/> is that frame, or the default when it is not.
    /// </summary>
    internal static bool TryFrame<TFrame>(RoomObjectDraw draw, Func<TFrame, bool> accepts, out TFrame frame) where TFrame : struct
    {
        if (draw is RoomObjectFrameDraw<TFrame> { Frame: var candidate } && accepts(candidate))
        {
            frame = candidate;
            return true;
        }
        frame = default;
        return false;
    }

    /// <summary>Whether <paramref name="draw"/> is one of <typeparamref name="TFrame"/>'s family, and its frame.</summary>
    internal static bool TryFrame<TFrame>(RoomObjectDraw draw, out TFrame frame) where TFrame : struct =>
        TryFrame(draw, static (TFrame _) => true, out frame);
}
