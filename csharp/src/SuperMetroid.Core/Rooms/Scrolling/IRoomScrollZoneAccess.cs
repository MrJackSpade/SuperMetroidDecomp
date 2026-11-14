namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The scroll states of the current room's screens, each named by its column and row in the room.</summary>
public interface IRoomScrollZoneAccess
{
    /// <summary>The scroll state of the room's <paramref name="screen"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The screen lies outside the room.</exception>
    RoomScrollState Read(Position16<RoomScreens> screen);

    /// <summary>Stores <paramref name="state"/> for the room's <paramref name="screen"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The screen lies outside the room.</exception>
    void Write(Position16<RoomScreens> screen, RoomScrollState state);
}
