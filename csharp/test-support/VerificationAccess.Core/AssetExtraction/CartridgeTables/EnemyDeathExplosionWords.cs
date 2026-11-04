using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The selectors that name an enemy's death explosion (#1472): an enemy header's +$22 word and the index into the
/// five-entry death-explosion table at $86:EFD5, decoded once here.
/// </summary>
internal static class EnemyDeathExplosionWords
{
    /// <summary>The selector of each death explosion.</summary>
    internal static readonly NativeWordIdentities<EnemyDeathExplosion> Selectors = new(new Dictionary<EnemyDeathExplosion, ushort>
    {
        [EnemyDeathExplosion.Small] = 0,
        [EnemyDeathExplosion.SamusContact] = 1,
        [EnemyDeathExplosion.Normal] = 2,
        [EnemyDeathExplosion.MiniKraid] = 3,
        [EnemyDeathExplosion.Big] = 4,
    });
}
