
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The exact projectile action a gate's script publishes to the enemy-projectile pool.</summary>
/// <param name="Operation">Whether to allocate a gate actor or wake an already associated actor; applying the request belongs to the runtime's projectile owner.</param>
/// <param name="Kind">The moving or initially closed gate projectile a Spawn allocates; null for Wake, which allocates nothing.</param>
/// <param name="PlmBlockIndex">Nonnegative row-major level-block ordinal for the gate's top cell, not a pixel coordinate or byte offset; each block is sixteen pixels and the association stores twice this index.</param>
public readonly record struct DownwardGateProjectileRequest(
    DownwardGateProjectileOperation Operation,
    DownwardGateProjectileKind? Kind,
    int PlmBlockIndex);
