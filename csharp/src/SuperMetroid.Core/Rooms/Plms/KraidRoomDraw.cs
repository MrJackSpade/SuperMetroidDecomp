namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The ten reachable Kraid ceiling and spike draw lists.</summary>
internal enum KraidRoomDraw
{
    /// <summary>First crumble frame, also used by the elevatube.</summary>
    CrumbleFirst,
    /// <summary>Second crumble frame.</summary>
    CrumbleSecond,
    /// <summary>Third crumble frame.</summary>
    CrumbleThird,
    /// <summary>Ceiling background-one final block.</summary>
    CeilingBackground1,
    /// <summary>Ceiling background-two final block.</summary>
    CeilingBackground2,
    /// <summary>Ceiling background-three final block.</summary>
    CeilingBackground3,
    /// <summary>Spike first-column final block.</summary>
    SpikeFirst,
    /// <summary>Spike second-column final block.</summary>
    SpikeSecond,
    /// <summary>Already-defeated ceiling clear, fifteen blocks.</summary>
    ClearCeiling,
    /// <summary>Already-defeated spikes clear, twenty-two blocks.</summary>
    ClearSpikes,
}
