namespace SuperMetroid.Core.Game;

/// <summary>Shared cartridge suit division for ordinary enemy and projectile damage.</summary>
public static class SamusSuitDamage
{
    /// <summary>Gravity takes precedence over Varia; integer shifts discard fractions.</summary>
    public static ushort Reduce(ushort damage, ushort equippedItems) =>
        (equippedItems & (ushort)SamusEquipmentFlags.GravitySuit) != 0 ? (ushort)(damage >> 2) :
        (equippedItems & (ushort)SamusEquipmentFlags.VariaSuit) != 0 ? (ushort)(damage >> 1) : damage;
}
