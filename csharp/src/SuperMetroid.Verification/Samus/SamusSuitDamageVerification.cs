using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Samus;

/// <summary>
/// Checks the suits' damage division (#1627): Gravity quarters and Varia halves any amount of damage, fractions dropped,
/// with no word bound on the damage. The hit every enemy, projectile and boss deals through it (#1632) leaves the
/// energy the suit-shift arithmetic it replaced left, and periodic damage's division gives the replaced 8.8 shift's
/// result.
/// </summary>
internal static class SamusSuitDamageVerification
{
    /// <summary>The four suit combinations: none, each suit alone, and both.</summary>
    private static readonly SamusEquipmentFlags[] SuitCombinations =
    [
        SamusEquipmentFlags.None,
        SamusEquipmentFlags.VariaSuit,
        SamusEquipmentFlags.GravitySuit,
        SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit,
    ];

    /// <summary>Divides damage within and past a word's range by each suit, and compares every hit with the replaced arithmetic.</summary>
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

        // Draygon's tail whip, Mother Brain's body and onion rings, the Beetom's drain, the Mochtroid's touch, the
        // grapple's hurting enemies, ordinary enemy touch and enemy projectiles each wrote this arithmetic out (#1632).
        // Every definition damage, with each suit combination and energies around the divided damage, keeps the energy
        // it left.
        int compared = 0, mismatches = 0;
        string? firstMismatch = null;
        foreach (SamusEquipmentFlags suits in SuitCombinations)
        {
            for (int damage = 0; damage <= ushort.MaxValue; damage++)
            {
                int divided = (suits & SamusEquipmentFlags.GravitySuit) != SamusEquipmentFlags.None
                    ? damage >> 2
                    : (suits & SamusEquipmentFlags.VariaSuit) != SamusEquipmentFlags.None
                        ? damage >> 1
                        : damage;
                foreach (int energy in new[] { 0, 1, 99, 1499, divided - 1, divided, divided + 1 })
                {
                    if (energy < 0)
                        continue;
                    int expected = energy <= divided ? 0 : energy - divided;
                    int actual = SamusSuitDamage.EnergyAfterHit(energy, damage, suits);
                    compared++;
                    if (actual == expected)
                        continue;
                    mismatches++;
                    firstMismatch ??= $"{suits}, damage {damage}, energy {energy}: expected {expected}, got {actual}";
                }
            }
        }
        AssertEqual(0, mismatches, $"every hit leaves the replaced arithmetic's energy ({compared} compared; first mismatch: {firstMismatch})");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusSuitDamage.EnergyAfterHit(99, -1, SamusEquipmentFlags.None),
            "a hit's damage is never negative");
        Console.WriteLine($"Samus suit damage: Gravity quarters and Varia halves damage of any size; {compared} hits leave the replaced arithmetic's energy.");
    }

    /// <summary>
    /// Compares the periodic damage a suit lets through with the 8.8 shift <c>ApplyPeriodicDamage</c> wrote out
    /// (#1632), for every fraction, the whole damage across and past the division's 256-point cycle, and each suit
    /// combination.
    /// </summary>
    internal static void VerifySamusPeriodicSuitDamage()
    {
        int[] wholes = [.. Enumerable.Range(0, 260), 511, 512, 32767, 32768, 65535];
        int compared = 0, mismatches = 0;
        string? firstMismatch = null;
        foreach (SamusEquipmentFlags suits in SuitCombinations)
        {
            int divisorShift = suits.HasAny(SamusEquipmentFlags.GravitySuit)
                ? 2
                : suits.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? 1
                    : 0;
            foreach (int whole in wholes)
            {
                for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
                {
                    int expectedWhole = whole, expectedFraction = fraction;
                    if (divisorShift != 0)
                    {
                        int overlapping = EightEight.FromSixteenSixteen(whole % 256, fraction) >> divisorShift;
                        expectedFraction = SixteenSixteen.FromEightEight(EightEight.FractionOf(overlapping));
                        expectedWhole = EightEight.WholeOf(overlapping);
                    }

                    UnsignedSixteenSixteen actual = SamusSuitDamage.ReducePeriodic(new UnsignedSixteenSixteen(whole, fraction), suits);
                    compared++;
                    if (actual.Whole == expectedWhole && actual.Fraction == expectedFraction)
                        continue;
                    mismatches++;
                    firstMismatch ??= $"{suits}, {whole}:{fraction}: expected {expectedWhole}:{expectedFraction}, got {actual.Whole}:{actual.Fraction}";
                }
            }
        }
        AssertEqual(0, mismatches, $"every periodic damage takes the replaced shift's division ({compared} compared; first mismatch: {firstMismatch})");
        Console.WriteLine($"Samus periodic suit damage: {compared} damages take the replaced 8.8 shift's division.");
    }
}
