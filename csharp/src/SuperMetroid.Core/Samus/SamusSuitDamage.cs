using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>Shared cartridge suit division for ordinary enemy and projectile damage.</summary>
public static class SamusSuitDamage
{
    /// <summary>Gravity takes precedence over Varia; integer shifts discard fractions.</summary>
    public static ushort Reduce(ushort damage, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems) =>
        (equippedItems & SamusEquipmentFlags.GravitySuit) != SamusEquipmentFlags.None ? (ushort)(damage >> 2) :
        (equippedItems & SamusEquipmentFlags.VariaSuit) != SamusEquipmentFlags.None ? (ushort)(damage >> 1) : damage;
}
