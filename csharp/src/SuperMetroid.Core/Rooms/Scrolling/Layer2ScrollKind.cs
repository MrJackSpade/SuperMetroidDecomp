namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>How one axis of the room's second background layer follows the camera.</summary>
public enum Layer2ScrollKind
{
    /// <summary>The axis copies layer one's scroll.</summary>
    CopiesLayer1,

    /// <summary>The axis stays at zero.</summary>
    Fixed,

    /// <summary>The axis scrolls at a fraction of layer one's speed (<see cref="Layer2AxisScroll.Factor"/> over 256).</summary>
    Parallax,
}
