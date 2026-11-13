using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks Mother Brain's finish-off energy thresholds (#1632) against the arithmetic they replaced: the stand-up
/// threshold, four suit-divided onion ring hits plus the margin, and the onion-rings-only threshold, one suit-divided
/// bomb hit plus the margin, give the thresholds the suit shifts of the replaced 80 and 160 gave for every suit
/// combination; and the onion ring's and bomb's definition damages are the 80 and 160 their collision and thresholds
/// wrote out.
/// </summary>
internal static class MotherBrainFinishOffThresholdVerification
{
    /// <summary>Compares both thresholds with each suit combination against the replaced shifts.</summary>
    internal static void VerifyMotherBrainFinishOffThreshold()
    {
        AssertEqual(80, MotherBrainAttackDamage.OnionRing, "the onion ring's damage is the 80 its collision dealt");
        AssertEqual(160, MotherBrainAttackDamage.Bomb, "the bomb's damage is the 160 the onion-rings-only threshold counted");

        SamusEquipmentFlags[] suitCombinations =
        [
            SamusEquipmentFlags.None,
            SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit,
            SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.VariaSuit,
        ];
        foreach (SamusEquipmentFlags suits in suitCombinations)
        {
            AssertEqual(ReplacedSuitDivision(80, suits) * 4 + 20, MotherBrainFinishSamusOffPhases.StandUpToFinishOffEnergy(suits),
                $"{suits}: the stand-up threshold");
            AssertEqual(ReplacedSuitDivision(160, suits) + 20, MotherBrainFinishSamusOffPhases.OnionRingsOnlyEnergy(suits),
                $"{suits}: the onion-rings-only threshold");
        }

        Console.WriteLine("Mother Brain finish-off thresholds: both thresholds with every suit combination give the replaced shifts' thresholds.");
    }

    /// <summary>The replaced suit shifts: Gravity quarters the damage, otherwise Varia halves it.</summary>
    private static int ReplacedSuitDivision(int damage, SamusEquipmentFlags suits) =>
        suits.HasAny(SamusEquipmentFlags.GravitySuit) ? damage >> 2
        : suits.HasAny(SamusEquipmentFlags.VariaSuit) ? damage >> 1
        : damage;
}
