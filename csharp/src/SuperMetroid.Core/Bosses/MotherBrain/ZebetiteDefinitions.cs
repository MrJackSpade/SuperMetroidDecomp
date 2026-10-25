using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed generation, health-tier, and respawn definitions for Tourian's Zebetites.</summary>
internal static class ZebetiteDefinitions
{
    /// <summary>The two colours the Zebetites pulse: the third sprite set's colours twelve and thirteen.</summary>
    public static readonly SpriteColourRun PulseColours = new(2, 12, 2);

    /// <summary>Native palette-cycle index mask applied to physical enemy slot zero.</summary>
    public const ushort PaletteCycleMask = 7;

    /// <summary>Embedded primary spawn record at <c>$A6:FCE1-$A6:FCF0</c>.</summary>
    private static readonly RoomEnemyPopulationRecord PrimarySpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), 0, new EnemyPropertyWord(EnemyProperties.ProcessInstructions), new EnemyExtraPropertyWord(EnemyExtraProperties.None), 0, 0);

    /// <summary>Embedded linked-half spawn record at <c>$A6:FCF9-$A6:FD08</c>.</summary>
    private static readonly RoomEnemyPopulationRecord LinkedSpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), 0, new EnemyPropertyWord(EnemyProperties.ProcessInstructions), new EnemyExtraPropertyWord(EnemyExtraProperties.None), 2, 0);

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
    internal static RoomEnemyPopulationRecord SpawnPopulation(ZebetiteHalf half) => half switch
    {
        ZebetiteHalf.Primary => PrimarySpawn,
        ZebetiteHalf.Linked => LinkedSpawn,
        _ => throw new InvalidOperationException($"Undefined {nameof(ZebetiteHalf)} {half}."),
    };
}
