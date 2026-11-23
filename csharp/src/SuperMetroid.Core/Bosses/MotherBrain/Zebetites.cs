using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Bosses.MotherBrain.ZebetiteDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Tourian's Zebetites: each active generation's barriers, the placement each half spawns from, and where each half stands.</summary>
internal static partial class Zebetites
{
    /// <summary>
    /// The barrier generation that stands once <paramref name="destroyed"/> Zebetites have been destroyed, or none once
    /// every one has been; the progress holds the count from zero to <see cref="PersistentProgress.ZebetiteCount"/>.
    /// </summary>
    internal static ZebetiteGenerationDefinition? StandingGeneration(int destroyed) =>
        destroyed < Generations.Count ? Generations[destroyed] : null;

    /// <summary>
    /// The placement the primary or linked half of <paramref name="generation"/> spawns from: at the place that half of
    /// that generation stands.
    /// </summary>
    internal static RoomEnemyPlacement SpawnPopulation(ZebetiteHalf half, ZebetiteGenerationDefinition generation)
    {
        return new(EnemyDefinitionId.Zebetite, EnemyPlacing.At(new(generation.X, YPosition(generation, half))), EnemyContact.None, EnemyProcessing.Animates,
            EnemyPresence.None, CollidesByFrameHitboxes: false, new ZebetiteSpawnSettings(half));
    }

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
