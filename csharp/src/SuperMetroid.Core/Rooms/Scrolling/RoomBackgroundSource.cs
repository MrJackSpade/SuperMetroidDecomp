namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>What a room state's background shows.</summary>
public enum RoomBackgroundSource
{
    /// <summary>The room's own level background, refreshed from its level data as the background scrolls.</summary>
    Level,

    /// <summary>What the room's owner places there: a library background, a boss's body, or nothing of the level.</summary>
    Placed,
}
