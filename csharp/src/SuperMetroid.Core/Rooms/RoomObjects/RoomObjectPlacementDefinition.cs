namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One room-object population entry: its block and its family's typed setup (#2383), which names the kind it places
/// (#2557).
/// </summary>
/// <param name="Block">The block it is placed on.</param>
/// <param name="Setup">The setup its family runs, with what the placement gives it.</param>
/// <param name="Site">Whether the block is one of the room's grid or of the terrain beyond the room.</param>
internal readonly record struct RoomObjectPlacementDefinition(
    Position16<RoomBlocks> Block, RoomObjectPlacementSetup Setup, RoomObjectSite Site = RoomObjectSite.RoomGrid);
