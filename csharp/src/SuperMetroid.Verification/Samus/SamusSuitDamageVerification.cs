using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Samus;

/// <summary>
/// Checks the suits' damage division (#1627): Gravity quarters and Varia halves any amount of damage, fractions dropped,
/// with no word bound on the damage.
/// </summary>
internal static class SamusSuitDamageVerification
{
    /// <summary>Divides damage within and past a word's range by each suit.</summary>
    internal static void VerifySamusSuitDamage()
    {
        foreach (int damage in new[] { 0, 1, 3, 65535, 70001 })
        {
            AssertEqual(damage, SamusSuitDamage.Reduce(damage, SamusEquipmentFlags.None), $"{damage}: no suit keeps the damage");
            AssertEqual(damage / 2, SamusSuitDamage.Reduce(damage, SamusEquipmentFlags.VariaSuit), $"{damage}: Varia halves it");
            AssertEqual(damage / 4, SamusSuitDamage.Reduce(damage, SamusEquipmentFlags.GravitySuit), $"{damage}: Gravity quarters it");
            AssertEqual(damage / 4, SamusSuitDamage.Reduce(damage, SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit),
                $"{damage}: Gravity takes precedence over Varia");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SamusSuitDamage.Reduce(-1, SamusEquipmentFlags.None), "damage is never negative");
        Console.WriteLine("Samus suit damage: Gravity quarters and Varia halves damage of any size.");
    }
}
