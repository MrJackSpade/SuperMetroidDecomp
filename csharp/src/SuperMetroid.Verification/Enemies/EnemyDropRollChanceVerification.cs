using SuperMetroid.Core.Enemies;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms that the chance a drop's chance roll is drawn again (#2510) decides every one of the 65536 samples exactly as
/// the test of the sample's lowest eight bits for zero it replaces did.
/// </summary>
internal static class EnemyDropRollChanceVerification
{
    /// <summary>Compares every sample's named answer with the test it replaces.</summary>
    internal static void VerifyEnemyDropRollChance()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 255) == 0, EnemyDropSelectionDefinitions.DropRollDrawnAgain.IsTakenBy(sample), "drop roll drawn again", sample);

        Console.WriteLine("Enemy drop roll chance: all 65536 samples decide as the test it replaces.");
    }
}
