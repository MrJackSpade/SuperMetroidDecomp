namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Values over a room pixel coordinate's fraction, as the game's terrain code applies them. Block and screen
/// offsets of a coordinate are <see cref="RoomGridConversions"/> operations.
/// </summary>
internal static class RoomPixelMasks
{
    /// <summary>The largest fraction: the position sits just short of the next whole pixel.</summary>
    internal const int LastFraction = ushort.MaxValue;
}
