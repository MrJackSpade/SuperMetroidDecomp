using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Verification's native view of Samus's knockback direction and contact damage (#1789): the numbers the cartridge keeps
/// for them. Core's values are arbitrary; a native snapshot or replay comparison translates through these tables.
/// </summary>
internal static class SamusMovementNativeWords
{
    /// <summary>The knockback direction word the cartridge keeps while Samus has no knockback direction.</summary>
    internal const ushort NoKnockbackDirection = 0;

    /// <summary>
    /// Samus's knockback from the cartridge's knockback direction word and whether its knockback movement runs: no direction
    /// is no knockback; a direction moves her while the knockback movement runs and is handed back once it has stopped.
    /// </summary>
    internal static SamusKnockback DecodeKnockback(ushort directionWord, bool knockbackMoving)
    {
        if (directionWord == NoKnockbackDirection)
            return SamusKnockback.None;
        SamusKnockbackDirection direction = KnockbackDirections.Decode(directionWord, "Samus knockback direction");
        return knockbackMoving ? new SamusKnockback.Moving(direction) : new SamusKnockback.HandedBack();
    }

    /// <summary>The cartridge's knockback direction word for <paramref name="knockback"/>.</summary>
    internal static ushort KnockbackDirectionWord(SamusKnockback knockback) => knockback switch
    {
        SamusKnockback.Moving moving => KnockbackDirections.Word(moving.Direction),
        SamusKnockback.HandedBack handedBack => KnockbackDirections.Word(handedBack.Direction),
        _ => NoKnockbackDirection,
    };

    /// <summary>
    /// The knockback table's index naming each <see cref="SamusKnockbackDirection"/>. Its straight-up entry, three, has no
    /// member, so a knockback word that reaches it is rejected.
    /// </summary>
    internal static readonly NativeWordIdentities<SamusKnockbackDirection> KnockbackDirections = new(new Dictionary<SamusKnockbackDirection, ushort>
    {
        [SamusKnockbackDirection.UpLeft] = 1,
        [SamusKnockbackDirection.UpRight] = 2,
        [SamusKnockbackDirection.DownLeft] = 4,
        [SamusKnockbackDirection.DownRight] = 5,
    });

    /// <summary>The stored word of Samus's body attack, which enemy touch handling reads.</summary>
    internal static readonly NativeWordIdentities<SamusBodyAttack> ContactDamage = new(new Dictionary<SamusBodyAttack, ushort>
    {
        [SamusBodyAttack.None] = 0,
        [SamusBodyAttack.SpeedBooster] = 1,
        [SamusBodyAttack.Shinespark] = 2,
        [SamusBodyAttack.ScrewAttack] = 3,
        [SamusBodyAttack.PseudoScrew] = 4,
    });
}
