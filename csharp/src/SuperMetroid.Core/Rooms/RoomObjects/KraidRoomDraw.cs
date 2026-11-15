namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The ten reachable Kraid ceiling and spike draw lists.</summary>
internal enum KraidRoomDraw
{
    /// <summary>First crumble frame, also used by the elevatube.</summary>
    CrumbleFirst,
    /// <summary>Second crumble frame.</summary>
    CrumbleSecond,
    /// <summary>Third crumble frame.</summary>
    CrumbleThird,
    /// <summary>The ceiling's background at its left edge, a crumbled block's final picture (#1895).</summary>
    CeilingBackgroundLeftEdge,
    /// <summary>The ceiling's background in an odd column, a crumbled block's final picture (#1895).</summary>
    CeilingBackgroundOddColumn,
    /// <summary>The ceiling's background in an even column, a crumbled block's final picture (#1895).</summary>
    CeilingBackgroundEvenColumn,
    /// <summary>Spike first-column final block.</summary>
    SpikeFirst,
    /// <summary>Spike second-column final block.</summary>
    SpikeSecond,
    /// <summary>Already-defeated ceiling clear, fifteen blocks.</summary>
    ClearCeiling,
    /// <summary>Already-defeated spikes clear, twenty-two blocks.</summary>
    ClearSpikes,
}
