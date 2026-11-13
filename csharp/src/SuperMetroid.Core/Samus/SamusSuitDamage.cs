using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>Shared suit division for ordinary enemy and projectile damage.</summary>
public static class SamusSuitDamage
{
    /// <summary>Gravity takes precedence over Varia; halving discards fractions. The damage is 0 through 65535.</summary>
    public static int Reduce(int damage, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, damage);
        return (equippedItems & SamusEquipmentFlags.GravitySuit) != SamusEquipmentFlags.None ? damage >> 2 :
            (equippedItems & SamusEquipmentFlags.VariaSuit) != SamusEquipmentFlags.None ? damage >> 1 : damage;
    }
}
