namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>One horizontal band of the scrolling sky (the ROM table at <c>$88:AEC1</c>).</summary>
/// <param name="TopPosition">The room-pixel row where the band begins, 0 through 65535; the band ends where the next one begins.</param>
/// <param name="Velocity">How far the band's horizontal scroll moves each frame, in 16.16 pixels.</param>
/// <param name="DataSlot">The horizontal-scroll accumulator the band adds its velocity to and reads its scroll from.</param>
public readonly record struct SkyScrollSection(
    int TopPosition,
    SignedSixteenSixteen Velocity,
    int DataSlot)
{
    /// <summary>The band's first room-pixel row, validated to the sixteen-bit row range world rows are compared in.</summary>
    public int TopPosition { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, TopPosition);
}
