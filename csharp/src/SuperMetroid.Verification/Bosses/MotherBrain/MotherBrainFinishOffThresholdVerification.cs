using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks Mother Brain's finish-off energy thresholds (#1632) against the suit-shift arithmetic they replaced: each
/// nominal damage, divided as the suits divide it, gives the threshold the shifts gave for every suit combination.
/// </summary>
internal static class MotherBrainFinishOffThresholdVerification
{
    /// <summary>Compares both nominal damages' thresholds with each suit combination against the replaced shifts.</summary>
    internal static void VerifyMotherBrainFinishOffThreshold()
    {
        SamusEquipmentFlags[] suitCombinations =
        [
            SamusEquipmentFlags.None,
            SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit,
            SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit,
        ];
        foreach (SamusEquipmentFlags suits in suitCombinations)
        {
            foreach (int nominalDamage in new[] { 80, 160 })
            {
                int divided = suits.HasAny(SamusEquipmentFlags.GravitySuit)
                    ? nominalDamage >> 2
                    : suits.HasAny(SamusEquipmentFlags.VariaSuit)
                        ? nominalDamage >> 1
                        : nominalDamage;
                int expected = nominalDamage == 80 ? divided * 4 + 20 : divided + 20;
                AssertEqual(expected, MotherBrainFinishSamusOffPhases.CalculateFinishOffHealthThreshold(suits, nominalDamage),
                    $"{suits}: the {nominalDamage}-damage finish-off threshold");
            }
        }

        Console.WriteLine("Mother Brain finish-off thresholds: both nominal damages with every suit combination give the replaced shifts' thresholds.");
    }
}
