using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Verification's native view of the Spore Spawn identities (#1413): the cartridge routine words naming each member.
/// Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SporeSpawnNativeWords
{
    /// <summary>The bank-$A5 routine word naming each <see cref="SporeSpawnMovement"/>.</summary>
    internal static readonly NativeWordIdentities<SporeSpawnMovement> SporeSpawnMovements = new(new Dictionary<SporeSpawnMovement, ushort>
    {
        [SporeSpawnMovement.Idle] = 0xeb1a,
        [SporeSpawnMovement.Descending] = 0xeb1b,
        [SporeSpawnMovement.Moving] = 0xeb52,
        [SporeSpawnMovement.SetupDeath] = 0xeb9b,
        [SporeSpawnMovement.Dying] = 0xebee,
    });
}
