namespace SuperMetroid.Core.Game;

/// <summary>Fixed generation, health-tier, and respawn definitions for Tourian's Zebetites.</summary>
internal static class ZebetiteDefinitions
{
    /// <summary>Eight two-color pulse records at $A6:FD87..FDA6.</summary>
    public const int PaletteSource = 0xa6fd87;

    /// <summary>Sprite palette two colors C..D, selected by $A6:FD7C.</summary>
    public const int PaletteDestinationColor = 0x0158 / sizeof(ushort);

    /// <summary>Native palette-cycle index mask applied to physical enemy slot zero.</summary>
    public const ushort PaletteCycleMask = 7;

    /// <summary>Embedded primary spawn record at <c>$A6:FCE1-$A6:FCF0</c>.</summary>
    private static readonly RoomEnemyPopulationRecord PrimarySpawn =
        new(EnemyDefinitionId.Zebetite, 0, 0, 0, 0x2000, 0, 0, 0);

    /// <summary>Embedded linked-half spawn record at <c>$A6:FCF9-$A6:FD08</c>.</summary>
    private static readonly RoomEnemyPopulationRecord LinkedSpawn =
        new(EnemyDefinitionId.Zebetite, 0, 0, 0, 0x2000, 0, 2, 0);

    /// <summary>
    /// $A6:FC03-FC32: four alternating large/split barriers, spaced twelve tiles
    /// leftward from X=824. Large barriers have half-height24 at Y=111; split
    /// halves have half-height8 at equal forty-pixel offsets from that center.
    /// </summary>
    internal static ZebetiteGenerationDefinition Generation(ushort generation)
    {
        if (generation >= 4)
        {
            throw new InvalidDataException(
                $"Zebetite generation {generation} exceeds its four active records.");
        }

        // Alternating single barriers and split pairs share a horizontal centerline.
        bool linkedPair = (generation & 1) != 0;
        int separation = linkedPair ? 40 : 0;
        return new(
            linkedPair ? (ushort)0x8000 : (ushort)0,
            linkedPair ? (ushort)8 : (ushort)24,
            HealthInstruction(linkedPair, 1000),
            (ushort)(824 - generation * 192),
            (ushort)(111 - separation),
            (ushort)(111 + separation));
    }

    /// <summary>Returns the animation list selected by multipart form and current health.</summary>
    internal static ushort HealthInstruction(bool linkedPair, ushort health)
    {
        int tier = health < 200 ? 4 :
            health < 400 ? 3 :
            health < 600 ? 2 :
            health < 800 ? 1 : 0;
        return ZebetiteInstructionProgramDefinitions.ProgramAt((linkedPair ? 5 : 0) + tier).Entry;
    }

    /// <summary>Returns the exact embedded primary or linked-half enemy population record.</summary>
    internal static RoomEnemyPopulationRecord SpawnPopulation(bool linkedHalf) =>
        linkedHalf ? LinkedSpawn : PrimarySpawn;
}

/// <summary>One active Zebetite generation's geometry and initial presentation binding.</summary>
internal readonly record struct ZebetiteGenerationDefinition(
    ushort GenerationFlags,
    ushort YRadius,
    ushort InstructionList,
    ushort XPosition,
    ushort PrimaryYPosition,
    ushort LinkedYPosition)
{
    /// <summary>Returns the Y coordinate for the primary or linked physical half.</summary>
    internal ushort YPosition(bool linkedHalf) =>
        linkedHalf ? LinkedYPosition : PrimaryYPosition;
}
