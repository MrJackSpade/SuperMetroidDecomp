using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Bosses.MotherBrain.ZebetiteDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Tourian's Zebetites: each active generation's barriers, the placement each half spawns from, and where each half stands.</summary>
internal static class Zebetites
{
    /// <summary>The barrier generation that stands once <paramref name="destroyed"/> Zebetites have been destroyed.</summary>
    /// <exception cref="InvalidDataException">Every barrier has been destroyed, or the count is negative.</exception>
    internal static ZebetiteGenerationDefinition Generation(int destroyed) =>
        destroyed is >= 0 and < PersistentProgress.ZebetiteCount
            ? Generations[destroyed]
            : throw new InvalidDataException($"Zebetite generation {destroyed} exceeds its four active generations.");

    /// <summary>The placement the primary or linked half spawns from.</summary>
    internal static RoomEnemyPlacement SpawnPopulation(ZebetiteHalf half) =>
        SpawnPopulations.TryGetValue(half, out RoomEnemyPlacement placement)
            ? placement
            : throw new InvalidOperationException($"Undefined {nameof(ZebetiteHalf)} {half}.");

    /// <summary>
    /// The room pixel row a generation's <paramref name="half"/> stands on: a single barrier on the centre line, a split
    /// pair's primary half above it and its linked half below it.
    /// </summary>
    internal static int YPosition(ZebetiteGenerationDefinition generation, ZebetiteHalf half) => generation.Form switch
    {
        ZebetiteBarrierForm.Single => generation.CentreY,
        ZebetiteBarrierForm.SplitPair => half == ZebetiteHalf.Linked ? generation.CentreY + SplitHalfOffset : generation.CentreY - SplitHalfOffset,
        _ => throw new InvalidDataException($"Undefined {nameof(ZebetiteBarrierForm)} {generation.Form}."),
    };
}
