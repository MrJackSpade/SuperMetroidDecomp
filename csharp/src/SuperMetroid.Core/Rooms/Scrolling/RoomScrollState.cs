namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// What a room's screen lets the camera do (#1741): stay out of it, enter it, or enter it with the camera's lowest position
/// aligned to the screen's top. Its values are arbitrary; the camera starts every screen closed.
/// </summary>
public enum RoomScrollState
{
    /// <summary>The camera may not move into the screen: it is a camera boundary.</summary>
    Closed,

    /// <summary>
    /// The camera may move into the screen, and its lowest position aligns the playfield to the screen's top rather than
    /// the 31 pixels lower that <see cref="Open"/> allows.
    /// </summary>
    OpenAlignedToTop,

    /// <summary>The camera may move into the screen, down to 31 pixels past the screen's top.</summary>
    Open,
}
