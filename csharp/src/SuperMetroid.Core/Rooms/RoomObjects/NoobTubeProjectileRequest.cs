namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One room-graphics enemy-projectile spawn emitted by the n00b-tube room object.</summary>
/// <param name="Definition">Which tube piece spawns: the crack, a glass shard or a released-air bubble.</param>
/// <param name="Piece">Which shard or bubble of the burst spawns, counted from zero in burst order; null for the crack, which is the only one.</param>
/// <param name="RoomObjectBlockIndex">Row-major room-cell index of the spawning room object.</param>
public readonly record struct NoobTubeProjectileRequest(
    NoobTubeProjectileDefinition Definition,
    int? Piece,
    int RoomObjectBlockIndex);
