using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The suits' damage division: Gravity quarters the damage Samus takes and Varia halves it. Ordinary enemy, projectile
/// and boss hits take it through <see cref="Hurt"/>; periodic damage takes it through <see cref="ReducePeriodic"/>.
/// </summary>
public static class SamusSuitDamage
{
    /// <summary>
    /// Gravity takes precedence over Varia; halving discards fractions. The damage is any amount of energy, zero or more
    /// (#1627).
    /// </summary>
    public static int Reduce(int damage, SamusEquipmentFlags equippedItems) =>
        Ensure.AtLeastZero(damage) / Divisor(equippedItems);

    /// <summary>What the equipped suit divides damage by: Gravity's divisor before Varia's, or one with neither.</summary>
    private static int Divisor(SamusEquipmentFlags equippedItems) =>
        (equippedItems & SamusEquipmentFlags.GravitySuit) != SamusEquipmentFlags.None ? SamusSuitDamageDefinitions.GravityDivisor :
            (equippedItems & SamusEquipmentFlags.VariaSuit) != SamusEquipmentFlags.None ? SamusSuitDamageDefinitions.VariaDivisor : 1;

    /// <summary>
    /// Hurts Samus by <paramref name="damageBeforeSuit"/>, zero or more, after her suit's division (<see cref="Reduce"/>):
    /// her energy drops by what the suit lets through and stops at zero. Each kind of hit sets its invincibility and
    /// knockback its own way.
    /// </summary>
    public static void Hurt(SamusState samus, int damageBeforeSuit)
    {
        Ensure.NotNull(samus);
        samus.Health = EnergyAfterHit(samus.Health, damageBeforeSuit, samus.EquippedItems);
    }

    /// <summary>
    /// The energy Samus keeps from <paramref name="energy"/> after a hit of <paramref name="damageBeforeSuit"/>, zero or
    /// more, that her suit divides: the energy less the divided damage, or zero when that damage takes all of it.
    /// </summary>
    internal static int EnergyAfterHit(int energy, int damageBeforeSuit, SamusEquipmentFlags equippedItems)
    {
        int damage = Reduce(damageBeforeSuit, equippedItems);
        return energy <= damage
            ? 0
            : energy - damage;
    }

    /// <summary>
    /// The periodic damage a suit lets through. Its rule differs from <see cref="Reduce"/>'s because periodic damage
    /// carries a fraction: a suit divides the damage as one amount in 256ths of energy, so a whole point the division
    /// moves into the fraction is kept and the fraction's part finer than a 256th drops out. Without a dividing suit the
    /// damage passes unchanged.
    /// </summary>
    /// <remarks>
    /// A dividing suit divides the whole damage of an update, its fraction included (#1795); the suit pickup equips its
    /// suit before it starts, which stops heat, and its rooms hold no other source of periodic damage.
    /// </remarks>
    public static EnergyAmount ReducePeriodic(EnergyAmount damage, SamusEquipmentFlags equippedItems)
    {
        if (!equippedItems.HasAny(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit))
            return damage;

        return damage.DividedAsSuitsDivide(Divisor(equippedItems));
    }
}
