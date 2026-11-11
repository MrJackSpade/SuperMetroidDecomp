
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The exact projectile action a gate's script publishes to the enemy-projectile pool.</summary>
/// <param name="Operation">Whether to allocate a gate actor or wake an already associated actor; applying the request belongs to the runtime's projectile owner.</param>
/// <param name="Kind">The moving or initially closed gate projectile a Spawn allocates; null for Wake, which allocates nothing.</param>
/// <param name="RoomObjectBlock">The room block of the gate's top block, where its room object sits. A Wake finds the gate projectile spawned for the same block.</param>
public readonly record struct DownwardGateProjectileRequest(
    DownwardGateProjectileOperation Operation,
    DownwardGateProjectileKind? Kind,
    Position16<RoomBlocks> RoomObjectBlock);
