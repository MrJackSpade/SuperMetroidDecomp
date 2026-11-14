using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Verification's native view of Samus's knockback direction and contact damage (#1789): the numbers the cartridge keeps
/// for them. Core's values are arbitrary; a native snapshot or replay comparison translates through these tables.
/// </summary>
internal static class SamusMovementNativeWords
{
    /// <summary>
    /// The knockback table's index naming each <see cref="SamusKnockbackDirection"/>. Its straight-up entry, three, has no
    /// member, so a knockback word that reaches it is rejected.
    /// </summary>
    internal static readonly NativeWordIdentities<SamusKnockbackDirection> KnockbackDirections = new(new Dictionary<SamusKnockbackDirection, ushort>
    {
        [SamusKnockbackDirection.None] = 0,
        [SamusKnockbackDirection.UpLeft] = 1,
        [SamusKnockbackDirection.UpRight] = 2,
        [SamusKnockbackDirection.DownLeft] = 4,
        [SamusKnockbackDirection.DownRight] = 5,
    });

    /// <summary>The contact-damage index the enemy touch handling selects Samus's attacking body by.</summary>
    internal static readonly NativeWordIdentities<SamusContactDamageIndex> ContactDamage = new(new Dictionary<SamusContactDamageIndex, ushort>
    {
        [SamusContactDamageIndex.None] = 0,
        [SamusContactDamageIndex.SpeedBooster] = 1,
        [SamusContactDamageIndex.Shinespark] = 2,
        [SamusContactDamageIndex.ScrewAttack] = 3,
        [SamusContactDamageIndex.PseudoScrew] = 4,
    });
}
