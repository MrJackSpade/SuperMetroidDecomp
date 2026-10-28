namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One compiled PLM population record: its header, block and room argument.</summary>
/// <param name="Header">The PLM header.</param>
/// <param name="Block">The block it is placed on.</param>
/// <param name="Argument">The decoded room argument, or null.</param>
/// <param name="CannonControl">The cannon-control word a Draygon wall cannon reads and writes; null for every other PLM.</param>
/// <param name="ScrollProgram">The retail scroll program a scroll trigger runs; null for every other PLM.</param>
internal readonly record struct RoomPlmPlacementDefinition(
    PlmHeaderId Header, Position16<RoomBlocks> Block, PlmRoomArgument? Argument, DraygonCannonControlWord? CannonControl = null,
    RoomScrollProgram? ScrollProgram = null);
