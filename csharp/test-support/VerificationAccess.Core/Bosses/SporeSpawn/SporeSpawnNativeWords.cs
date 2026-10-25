using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Verification's native view of the Spore Spawn identities (#1413): the cartridge routine words naming each member.
/// Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SporeSpawnNativeWords
{
    /// <summary>The bank-$A5 routine word naming each <see cref="SporeSpawnFunction"/>.</summary>
    internal static readonly NativeWordIdentities<SporeSpawnFunction> SporeSpawnFunctions = new(new Dictionary<SporeSpawnFunction, ushort>
    {
        [SporeSpawnFunction.Idle] = 0xeb1a,
        [SporeSpawnFunction.Descending] = 0xeb1b,
        [SporeSpawnFunction.Moving] = 0xeb52,
        [SporeSpawnFunction.SetupDeath] = 0xeb9b,
        [SporeSpawnFunction.Dying] = 0xebee,
    });
}
