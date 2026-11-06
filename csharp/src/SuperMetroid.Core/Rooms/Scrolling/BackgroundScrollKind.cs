namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>How one axis of the room's second background layer follows the camera.</summary>
public enum BackgroundScrollKind
{
    /// <summary>The axis copies layer one's scroll.</summary>
    FollowsForeground,

    /// <summary>The axis stays at zero.</summary>
    Fixed,

    /// <summary>The axis scrolls at a fraction of layer one's speed (<see cref="BackgroundAxisScroll.Factor"/> over 256).</summary>
    Parallax,
}
