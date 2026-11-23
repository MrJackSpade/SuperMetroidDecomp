using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's native view of <see cref="EnemyPickupKind"/> (#1546): the selector the drop selection returns for
/// each member, which indexes the bank-$86 pickup program table and fills the probability-column table. Core's values
/// are arbitrary; a ROM oracle translates through this table. Selector zero, the program table's empty first entry, names
/// no kind: the drop selection never returns it. The selection's nothing-drops outcome is <see cref="NothingSelector"/>.
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
    });

    /// <summary>The native drop selector of the roll's nothing-drops outcome.</summary>
    internal const ushort NothingSelector = 6;

    /// <summary>The drop roll's outcome a native drop selector names: nothing, or the pickup it selects.</summary>
    /// <exception cref="InvalidDataException">The selector names no outcome.</exception>
    internal static EnemyDropRoll Outcome(ushort selector) =>
        selector == NothingSelector ? EnemyDropRoll.Nothing : EnemyDropRoll.Pickup(Selectors.Decode(selector, "enemy drop selector"));
}
