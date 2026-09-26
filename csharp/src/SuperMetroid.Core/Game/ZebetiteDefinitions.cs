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

    /// <summary>
    /// Four parallel generation rows at <c>$A6:FC03-$A6:FC32</c>: multipart flag,
    /// collision half-height, initial list, X, primary Y, and linked-half Y.
    /// </summary>
    private static readonly ZebetiteGenerationDefinition[] Generations =
    [
        new(0x0000, 0x0018,
            ZebetiteInstructionProgramDefinitions.BigHealthAtLeast800,
            0x0338, 0x006f, 0x006f),
        new(0x8000, 0x0008,
            ZebetiteInstructionProgramDefinitions.SmallHealthAtLeast800,
            0x0278, 0x0047, 0x0097),
        new(0x0000, 0x0018,
            ZebetiteInstructionProgramDefinitions.BigHealthAtLeast800,
            0x01b8, 0x006f, 0x006f),
        new(0x8000, 0x0008,
            ZebetiteInstructionProgramDefinitions.SmallHealthAtLeast800,
            0x00f8, 0x0047, 0x0097),
    ];

    /// <summary>
    /// Big-barrier health-tier instruction lists at <c>$A6:FD4A-$A6:FD53</c>, ordered
    /// from at least 800 HP through less than 200 HP.
    /// </summary>
    private static readonly ushort[] BigHealthInstructionLists =
    [
        ZebetiteInstructionProgramDefinitions.BigHealthAtLeast800,
        ZebetiteInstructionProgramDefinitions.BigHealthBelow800,
        ZebetiteInstructionProgramDefinitions.BigHealthBelow600,
        ZebetiteInstructionProgramDefinitions.BigHealthBelow400,
        ZebetiteInstructionProgramDefinitions.BigHealthBelow200,
    ];

    /// <summary>
    /// Linked-pair health-tier instruction lists at <c>$A6:FD54-$A6:FD5D</c>, ordered
    /// from at least 800 HP through less than 200 HP.
    /// </summary>
    private static readonly ushort[] LinkedHealthInstructionLists =
    [
        ZebetiteInstructionProgramDefinitions.SmallHealthAtLeast800,
        ZebetiteInstructionProgramDefinitions.SmallHealthBelow800,
        ZebetiteInstructionProgramDefinitions.SmallHealthBelow600,
        ZebetiteInstructionProgramDefinitions.SmallHealthBelow400,
        ZebetiteInstructionProgramDefinitions.SmallHealthBelow200,
    ];

    /// <summary>Embedded primary spawn record at <c>$A6:FCE1-$A6:FCF0</c>.</summary>
    private static readonly RoomEnemyPopulationRecord PrimarySpawn =
        new(0xe27f, 0, 0, 0, 0x2000, 0, 0, 0);

    /// <summary>Embedded linked-half spawn record at <c>$A6:FCF9-$A6:FD08</c>.</summary>
    private static readonly RoomEnemyPopulationRecord LinkedSpawn =
        new(0xe27f, 0, 0, 0, 0x2000, 0, 2, 0);

    /// <summary>Returns one of the four active cartridge generation rows.</summary>
    internal static ZebetiteGenerationDefinition Generation(ushort generation)
    {
        if (generation >= Generations.Length)
        {
            throw new InvalidDataException(
                $"Zebetite generation {generation} exceeds its four active records.");
        }

        return Generations[generation];
    }

    /// <summary>Returns the animation list selected by multipart form and current health.</summary>
    internal static ushort HealthInstruction(bool linkedPair, ushort health)
    {
        int tier = health < 200 ? 4 :
            health < 400 ? 3 :
            health < 600 ? 2 :
            health < 800 ? 1 : 0;
        return (linkedPair ? LinkedHealthInstructionLists : BigHealthInstructionLists)[tier];
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
