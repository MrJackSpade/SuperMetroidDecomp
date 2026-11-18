namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One compiled room-object population entry: its kind, block and its family's typed setup (#2383).</summary>
/// <param name="Kind">The kind of room object placed.</param>
/// <param name="Block">The block it is placed on.</param>
/// <param name="Setup">The setup its family runs, with what the placement gives it.</param>
/// <param name="Site">Whether the block is one of the room's grid or of the terrain beyond the room.</param>
internal readonly record struct RoomObjectPlacementDefinition(
    RoomObjectKind Kind, Position16<RoomBlocks> Block, RoomObjectPlacementSetup Setup, RoomObjectSite Site = RoomObjectSite.RoomGrid);
