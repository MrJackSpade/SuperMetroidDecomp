namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One piece of the n00b tube's break, spawned by the n00b-tube room object.</summary>
/// <param name="Piece">The piece that spawns: the crack, a glass shard or a released-air bubble.</param>
/// <param name="RoomObjectBlock">The room block of the spawning room object.</param>
public readonly record struct NoobTubeProjectileRequest(
    NoobTubePiece Piece,
    Position16<RoomBlocks> RoomObjectBlock);
