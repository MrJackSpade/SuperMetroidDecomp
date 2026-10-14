using SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Room scroll-zone writes a focused enemy fixture records (#633).</summary>
internal sealed class FixtureRoomScroll(Action<int, RoomScrollState> write) : IRoomScrollZoneAccess
{
    /// <summary>Fails because focused enemy fixtures install write observation only.</summary>
    public RoomScrollState Read(int index) =>
        throw new InvalidOperationException("The enemy fixture did not install room scroll reads.");

    /// <summary>Forwards a scroll-zone mutation to the fixture's recorder.</summary>
    public void Write(int index, RoomScrollState state) => write(index, state);
}
