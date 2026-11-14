using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's native view of <see cref="EnemyPickupKind"/> (#1546): the selector the drop selection returns for
/// each member, which indexes the bank-$86 pickup program table and fills the probability-column table. Core's values
/// are arbitrary; a ROM oracle translates through this table. Selector zero, the program table's empty first entry, names
/// no kind: the drop selection never returns it.
/// </summary>
internal static class EnemyPickupNativeWords
{
    /// <summary>The native drop selector of each pickup kind.</summary>
    internal static readonly NativeWordIdentities<EnemyPickupKind> Selectors = new(new Dictionary<EnemyPickupKind, ushort>
    {
        [EnemyPickupKind.SmallEnergy] = 1,
        [EnemyPickupKind.BigEnergy] = 2,
        [EnemyPickupKind.PowerBomb] = 3,
        [EnemyPickupKind.Missile] = 4,
        [EnemyPickupKind.SuperMissile] = 5,
        [EnemyPickupKind.NoDrop] = 6,
    });
}
