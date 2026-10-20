namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One room PLM a boss spawns at a block.</summary>
/// <param name="Block">The room block the PLM is created at.</param>
/// <param name="Header">The PLM header spawned there.</param>
public readonly record struct PlmSpawnRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);
