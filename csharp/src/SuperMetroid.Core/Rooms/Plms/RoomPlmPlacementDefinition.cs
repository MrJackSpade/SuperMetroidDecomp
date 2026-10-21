namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One compiled PLM population record: its header, block and room argument.</summary>
/// <param name="Header">The PLM header.</param>
/// <param name="Block">The block it is placed on.</param>
/// <param name="Argument">The room argument.</param>
/// <param name="CannonControl">The cannon-control word a Draygon wall cannon reads and writes; null for every other PLM.</param>
internal readonly record struct RoomPlmPlacementDefinition(
    PlmHeaderId Header, Position16<RoomBlocks> Block, int Argument, DraygonCannonControlWord? CannonControl = null);
