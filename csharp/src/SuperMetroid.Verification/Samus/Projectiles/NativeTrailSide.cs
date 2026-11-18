namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// One sparkle of the native trail interpreter the trail checks run beside the port's (#2458): the words native trail
/// RAM keeps for it, its program pointer, its timer and its position.
/// </summary>
internal sealed class NativeTrailSide
{
    /// <summary>The bank-$90 address of the program word the side reads next.</summary>
    internal int InstructionPointer { get; set; }

    /// <summary>The side's timer: the updates its frame has left, zero once its program has ended.</summary>
    internal int FrameHold { get; set; }

    /// <summary>The side's room-pixel position.</summary>
    internal Position16<RoomPixels> Position { get; set; }

    /// <summary>Starts the side at <paramref name="start"/>, its timer at one, at <paramref name="position"/>.</summary>
    internal static NativeTrailSide StartingAt(ushort start, Position16<RoomPixels> position) =>
        new() { InstructionPointer = start, FrameHold = 1, Position = position };
}
