namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The five n00b-tube draws, named for what they show as the tube breaks (#2127).</summary>
internal enum NoobTubeDraw
{
    /// <summary>The intact glass, the block a Power Bomb strikes.</summary>
    Intact,
    /// <summary>The glass as it cracks, solid; the crack is drawn over it.</summary>
    Cracking,
    /// <summary>The tube's upper three rows opened, from its rim down.</summary>
    UpperRowsOpen,
    /// <summary>The glass gone and the tube's lower three rows opened, down to its floor.</summary>
    LowerRowsOpen,
    /// <summary>The open tube's solid upper rim, shown while the water returns.</summary>
    Opened,
}
