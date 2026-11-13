using SuperMetroid.Core.Enemies.Metroids;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// Confirms that the named phase of a bombed Metroid's escape countdown (#1619) picks, at every count, exactly the
/// displacement the mask it replaces did: the countdown's two lowest bits indexing the four displacements.
/// </summary>
internal static class MetroidCounterPhasesVerification
{
    /// <summary>Compares every count, negative ones included, with the replaced mask expression.</summary>
    internal static void VerifyMetroidCounterPhases()
    {
        for (int countdown = short.MinValue; countdown <= ushort.MaxValue; countdown++)
            AssertSameAt(MetroidBehaviorDefinitions.EscapeDisplacements[countdown & 3], MetroidReactions.EscapeDisplacement(countdown), "escape displacement", countdown);

        AssertEqual(MetroidBehaviorDefinitions.EscapeDisplacements.Length, MetroidCadenceDefinitions.EscapeDisplacementStep.Outcomes, "the escape step spans the four displacements");
        Console.WriteLine("Metroid counter phases: every escape count picks its displacement as the mask it replaces.");
    }
}
