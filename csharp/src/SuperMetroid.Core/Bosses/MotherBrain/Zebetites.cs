using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.MotherBrain.ZebetiteDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Tourian's Zebetites: each active generation's barriers, the spawn record of each half, and where each half stands.</summary>
internal static class Zebetites
{
    /// <summary>
    /// $A6:FC03-FC32: four alternating large/split barriers, spaced twelve tiles
    /// leftward from X=824. Large barriers have half-height24 at Y=111; split
    /// halves have half-height8 at equal forty-pixel offsets from that center.
    /// </summary>
    internal static ZebetiteGenerationDefinition Generation(int generation)
    {
        if ((uint)generation >= 4)
        {
            throw new InvalidDataException(
                $"Zebetite generation {generation} exceeds its four active records.");
        }

        // Alternating single barriers and split pairs share a horizontal centerline.
        bool linkedPair = (generation & 1) != 0;
        int separation = linkedPair ? 40 : 0;
        return new(
            linkedPair ? ZebetiteGenerationFlags.LinkedPair : ZebetiteGenerationFlags.None,
            linkedPair ? 8 : 24,
            ZebetiteAnimations.HealthProgram(linkedPair, 1000),
            824 - generation * 192,
            111 - separation,
            111 + separation);
    }

    /// <summary>Returns the exact embedded primary or linked-half enemy population record.</summary>
    internal static RoomEnemyPopulationRecord SpawnPopulation(ZebetiteHalf half) =>
        SpawnPopulations.TryGetValue(half, out RoomEnemyPopulationRecord record)
            ? record
            : throw new InvalidOperationException($"Undefined {nameof(ZebetiteHalf)} {half}.");

    /// <summary>Returns the Y coordinate of a generation's primary or linked physical half.</summary>
    internal static int YPosition(ZebetiteGenerationDefinition generation, bool linkedHalf) =>
        linkedHalf ? generation.LinkedYPosition : generation.PrimaryYPosition;
}
