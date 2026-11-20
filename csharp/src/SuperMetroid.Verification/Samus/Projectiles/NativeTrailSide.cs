namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// One sparkle of the native trail interpreter the trail checks run beside the port's (#2458): the words native trail
/// RAM keeps for it, its program pointer, its timer and its position.
/// </summary>
internal sealed class NativeTrailSide
{
    /// <summary>
    /// How far up and left of its sparkle native keeps a sparkle's position and draws its eight-pixel picture from, in
    /// pixels: half the picture.
    /// </summary>
    internal const int PictureHalf = 4;

    /// <summary>The bank-$90 address of the program word the side reads next.</summary>
    internal int InstructionPointer { get; set; }

    /// <summary>The side's timer: the updates its frame has left, zero once its program has ended.</summary>
    internal int FrameHold { get; set; }

    /// <summary>
    /// The side's room-pixel position as native keeps it: its picture's top-left corner, <see cref="PictureHalf"/> up and
    /// left of the sparkle, where native draws the picture's OBJ.
    /// </summary>
    internal Position16<RoomPixels> Position { get; set; }

    /// <summary>
    /// Where the sparkle itself stands, <see cref="PictureHalf"/> down and right of <see cref="Position"/>: the place the
    /// port keeps, whose picture is drawn centred on it (#2573).
    /// </summary>
    internal Position16<RoomPixels> Sparkle => Position + new Offset16<RoomPixels>(PictureHalf, PictureHalf);

    /// <summary>
    /// Starts the side at <paramref name="start"/>, its timer at one, for a sparkle standing at <paramref name="sparkle"/>;
    /// native keeps its picture's corner, <see cref="PictureHalf"/> up and left of it.
    /// </summary>
    internal static NativeTrailSide StartingAt(ushort start, Position16<RoomPixels> sparkle) =>
        new() { InstructionPointer = start, FrameHold = 1, Position = sparkle - new Offset16<RoomPixels>(PictureHalf, PictureHalf) };
}
