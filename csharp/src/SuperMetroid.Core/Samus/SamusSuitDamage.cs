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
    /// carries a fraction: a suit divides the damage as one amount in 256ths of energy, so a whole point the division
    /// moves into the fraction is kept and the fraction's part finer than a 256th drops out. Without a dividing suit the
    /// damage passes unchanged.
    /// </summary>
    /// <remarks>
    /// The game divides only the whole damage's lowest eight bits, but no periodic damage reaches 256 whole points in one
    /// update, so the whole damage is divided as it is (#1795). The most an update's producers accumulate is a heavy
    /// spike's 60 or the swinging grapple's spike damage, never both, since each starts the invincibility the other waits
    /// out, with lava's half or acid's one and a half, heat's quarter, a chewing plant's two and the Draygon turret
    /// spike's one, all far below 256. The suit pickup, which holds the damage over its updates, equips its suit before it
    /// starts, which stops heat, and its rooms hold no other producer.
    /// </remarks>
    public static UnsignedSixteenSixteen ReducePeriodic(UnsignedSixteenSixteen damage, SamusEquipmentFlags equippedItems)
    {
        if (!equippedItems.HasAny(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit))
            return damage;

        int divided = Reduce(EightEight.FromSixteenSixteen(damage.Whole, damage.Fraction), equippedItems);
        return new UnsignedSixteenSixteen(EightEight.WholeOf(divided), SixteenSixteen.FromEightEight(EightEight.FractionOf(divided)));
    }
}
