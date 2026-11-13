using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The suits' damage division: Gravity quarters the damage Samus takes and Varia halves it. Ordinary enemy, projectile
/// and boss hits take it through <see cref="Hurt"/>; periodic damage takes it through <see cref="ReducePeriodic"/>.
/// </summary>
public static class SamusSuitDamage
{
    /// <summary>The whole damage points a suit's division of periodic damage reads within a cycle of 256; higher points drop out.</summary>
    private const int SuitDividedWholeDamageCycle = 256;

    /// <summary>
    /// Gravity takes precedence over Varia; halving discards fractions. The damage is any amount of energy, zero or more
    /// (#1627).
    /// </summary>
    public static int Reduce(int damage, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.AtLeastZero(damage);
        return (equippedItems & SamusEquipmentFlags.GravitySuit) != SamusEquipmentFlags.None ? damage >> 2 :
            (equippedItems & SamusEquipmentFlags.VariaSuit) != SamusEquipmentFlags.None ? damage >> 1 : damage;
    }

    /// <summary>
    /// Hurts Samus by <paramref name="damageBeforeSuit"/>, zero or more, after her suit's division (<see cref="Reduce"/>):
    /// her energy drops by what the suit lets through and stops at zero. The hit's invincibility and knockback are the
    /// caller's, since each kind of hit sets them its own way.
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
    public static int EnergyAfterHit(int energy, int damageBeforeSuit, SamusEquipmentFlags equippedItems)
    {
        int damage = Reduce(damageBeforeSuit, equippedItems);
        return energy <= damage
            ? 0
            : energy - damage;
    }

    /// <summary>
    /// The periodic damage a suit lets through. Its rule differs from <see cref="Reduce"/>'s because periodic damage
    /// carries a fraction: a suit divides the damage as one amount in 256ths of energy, the whole damage within its
    /// 256-point cycle and the fraction's 256ths, so a whole point the division moves into the fraction is kept and the
    /// fraction's finer part and the whole damage's higher points drop out. Without a dividing suit the damage passes
    /// unchanged.
    /// </summary>
    public static UnsignedSixteenSixteen ReducePeriodic(UnsignedSixteenSixteen damage, SamusEquipmentFlags equippedItems)
    {
        if (!equippedItems.HasAny(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit))
            return damage;

        int divided = Reduce(EightEight.FromSixteenSixteen(damage.Whole % SuitDividedWholeDamageCycle, damage.Fraction), equippedItems);
        return new UnsignedSixteenSixteen(EightEight.WholeOf(divided), SixteenSixteen.FromEightEight(EightEight.FractionOf(divided)));
    }
}
