using SuperMetroid.Core.Enemies.Metroids;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// Confirms that a bombed Metroid's escape moves, listed in the order they happen (#2343), pick at every count exactly
/// the displacement the cartridge's mask did: the countdown's two lowest bits indexing its table of four.
/// </summary>
internal static class MetroidCounterPhasesVerification
{
    /// <summary>The cartridge's four escape displacements in its table's order: right, up, left and down.</summary>
    private static readonly Offset16<RoomPixels>[] TableOrder = [new(2, 0), new(0, -2), new(-2, 0), new(0, 2)];

    /// <summary>
    /// Compares every count an escape reaches, the four it counts down from and every count a Metroid frozen through its
    /// escape's end goes on with, with the replaced mask expression.
    /// </summary>
    internal static void VerifyMetroidCounterPhases()
    {
        for (int countdown = short.MinValue; countdown <= MetroidBehaviorDefinitions.EscapeDuration; countdown++)
            AssertSameAt(TableOrder[countdown & 3], MetroidReactions.EscapeDisplacement(countdown), "escape displacement", countdown);

        AssertEqual(MetroidBehaviorDefinitions.EscapeDuration, MetroidBehaviorDefinitions.EscapeMoves.Length, "the escape makes one move an update");
        Console.WriteLine("Metroid counter phases: every escape count picks its displacement as the mask it replaces.");
    }
}
