
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The exact projectile action a gate's script publishes to the enemy-projectile pool.</summary>
/// <param name="Operation">Whether to allocate a gate actor or wake an already associated actor; applying the request belongs to the runtime's projectile owner.</param>
/// <param name="Kind">The moving or initially closed gate projectile a Spawn allocates; null for Wake, which allocates nothing.</param>
/// <param name="RoomObjectBlockIndex">The room cell of the gate's top block, as every room object names its cell: its place in the room's row-major grid of sixteen-pixel blocks. A Wake finds the gate projectile spawned for the same cell.</param>
public readonly record struct DownwardGateProjectileRequest(
    DownwardGateProjectileOperation Operation,
    DownwardGateProjectileKind? Kind,
    int RoomObjectBlockIndex);
