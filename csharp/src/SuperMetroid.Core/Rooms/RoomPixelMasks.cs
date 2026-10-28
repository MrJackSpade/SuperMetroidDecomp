namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Values over a 16-bit room pixel coordinate's fraction, as the cartridge's terrain code applies them. Block and screen
/// offsets of a coordinate are <see cref="RoomGridConversions"/> operations.
/// </summary>
internal static class RoomPixelMasks
{
    /// <summary>The largest fraction: the position sits just short of the next whole pixel.</summary>
    internal const ushort LastFraction = ushort.MaxValue;
}
