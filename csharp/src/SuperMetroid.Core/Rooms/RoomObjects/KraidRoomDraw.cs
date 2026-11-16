namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The pictures Kraid's room's crumbling ceiling and spikes leave, and the clears of an already-defeated room. A block's
/// crumble before it is <see cref="KraidRoomObjectDraws.CrumbleDraw"/>.
/// </summary>
internal enum KraidRoomDraw
{
    /// <summary>The ceiling's background at its left edge, a crumbled block's final picture (#1895).</summary>
    CeilingBackgroundLeftEdge,
    /// <summary>The ceiling's background in an odd column, a crumbled block's final picture (#1895).</summary>
    CeilingBackgroundOddColumn,
    /// <summary>The ceiling's background in an even column, a crumbled block's final picture (#1895).</summary>
    CeilingBackgroundEvenColumn,
    /// <summary>The background a crumbled spike leaves in the left column of a pass's two.</summary>
    SpikeBackgroundLeftColumn,
    /// <summary>The background a crumbled spike leaves in the right column of a pass's two.</summary>
    SpikeBackgroundRightColumn,
    /// <summary>Already-defeated ceiling clear, fifteen blocks.</summary>
    ClearCeiling,
    /// <summary>Already-defeated spikes clear, twenty-two blocks.</summary>
    ClearSpikes,
}
