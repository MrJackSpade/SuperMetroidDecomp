namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The current room's scroll-zone states.</summary>
public interface IRoomScrollZoneAccess
{
    /// <summary>The state of scroll zone <paramref name="index"/>.</summary>
    RoomScrollState Read(int index);

    /// <summary>Stores <paramref name="state"/> for scroll zone <paramref name="index"/>.</summary>
    void Write(int index, RoomScrollState state);
}
