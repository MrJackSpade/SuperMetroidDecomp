namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Physical bank-$84 draw lists selected by the n00b-tube PLM. The native
/// coroutine still owns power-bomb gating, shard release, room FX, the event
/// bit and Samus's input lock; these definitions only replace immutable draw
/// payload reads.
/// </summary>
internal static class NoobTubePlmDrawDefinitions
{
    /// <summary>The visual block of the tube's glass.</summary>
    internal const int GlassBlock = 0x140;
    /// <summary>The visual block of the tube's upper rim.</summary>
    internal const int RimBlock = 0x141;
    /// <summary>The visual block of the floor row's first edge cell; the second steps one block.</summary>
    internal const int FloorEdgeFirstBlock = 0x14e;
    /// <summary>The visual block of the top panel row's edge; lower rows step one block toward the centre.</summary>
    internal const int PanelFirstBlock = 0x322;
}
