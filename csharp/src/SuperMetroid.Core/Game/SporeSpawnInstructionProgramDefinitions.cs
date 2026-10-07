using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled mechanics words from Spore Spawn's five bank-$A5 instruction programs.
/// </summary>
/// <remarks>
/// These native lists interleave simulation state with presentation data. Durations,
/// callbacks, callback operands, loop counters, and branch targets affect gameplay and
/// therefore live here. The word following every duration is a spritemap pointer. The
/// forty-one presentation operands belong to the separate compiled visual-selector
/// catalog when artwork is installed; unbound native diagnostics can still read the bus.
/// </remarks>
internal abstract class SporeSpawnInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>$A5:E6B9</c>, defeated-room initialization program.</summary>
    internal const ushort InitialDead = 0xe6b9;

    /// <summary><c>$A5:E6C7</c>, living-room descent initialization program.</summary>
    internal const ushort InitialAlive = 0xe6c7;

    /// <summary><c>$A5:E6D5</c>, first open-and-moving combat program.</summary>
    internal const ushort FightStarted = 0xe6d5;

    /// <summary><c>$A5:E729</c>, close-head and resume-motion program.</summary>
    internal const ushort CloseAndMove = 0xe729;

    /// <summary><c>$A5:E77D</c>, complete death and hardening program.</summary>
    internal const ushort Death = 0xe77d;

    /// <summary>Number of mechanics words compiled from the five native programs.</summary>
    /// <summary><c>InstList_SporeSpawn_OpenAndStop_0</c> at $A5:E6E3.</summary>
    private const ushort SpawnOpenAndStop0 = 0xe6e3;
    /// <summary><c>InstList_SporeSpawn_OpenAndStop_1</c> at $A5:E715.</summary>
    private const ushort SpawnOpenAndStop1 = 0xe715;
    /// <summary><c>Instruction_SporeSpawn_IncreaseMaxXRadius</c> at $A5:E75F.</summary>
    private const ushort SpawnIncreaseMaxXRadius = 0xe75f;
    /// <summary><c>Instruction_SporeSpawn_ClearDamagedFlag</c> at $A5:E771.</summary>
    private const ushort SpawnClearDamagedFlag = 0xe771;
    /// <summary><c>InstList_SporeSpawn_DeathSequence_1</c> at $A5:E78D.</summary>
    private const ushort SpawnDeathSequence1 = 0xe78d;
    /// <summary><c>InstList_SporeSpawn_DeathSequence_2</c> at $A5:E7BD.</summary>
    private const ushort SpawnDeathSequence2 = 0xe7bd;
    /// <summary><c>Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta</c> at $A5:E82D.</summary>
    private const ushort SpawnSetMaxXRadiusAndAngleDelta = 0xe82d;
    /// <summary><c>Instruction_SporeSpawn_SporeGenerationFlagInY</c> at $A5:E872.</summary>
    private const ushort SpawnSporeGenerationFlagInY = 0xe872;
    /// <summary><c>Instruction_SporeSpawn_Harden</c> at $A5:E87C.</summary>
    private const ushort SpawnHarden = 0xe87c;
    /// <summary><c>Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6</c> at $A5:E895.</summary>
    private const ushort SpawnQueueSFXInYLib2Max6 = 0xe895;
    /// <summary><c>Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine</c> at $A5:E8B1.</summary>
    private const ushort SpawnCallSporeSpawnDeathItemDropRoutine = 0xe8b1;
    /// <summary><c>Instruction_SporeSpawn_FunctionInY</c> at $A5:E8BA.</summary>
    private const ushort SpawnFunctionInY = 0xe8ba;
    /// <summary><c>Instruction_SporeSpawn_LoadDeathSequencePalette</c> at $A5:E8CA.</summary>
    private const ushort SpawnLoadDeathSequencePalette = 0xe8ca;
    /// <summary><c>Instruction_SporeSpawn_LoadDeathSequenceTargetPalette</c> at $A5:E91C.</summary>
    private const ushort SpawnLoadDeathSequenceTargetPalette = 0xe91c;
    /// <summary><c>Instruction_SporeSpawn_SpawnHardeningDustCloud</c> at $A5:E96E.</summary>
    private const ushort SpawnSpawnHardeningDustCloud = 0xe96e;
    /// <summary><c>Instruction_SporeSpawn_SpawnDyingExplosion</c> at $A5:E9B1.</summary>
    private const ushort SpawnSpawnDyingExplosion = 0xe9b1;
    /// <summary><c>RTS_A5EB1A</c> at $A5:EB1A.</summary>
    private const ushort EmptyRoutineEB1A = 0xeb1a;
    /// <summary><c>Function_SporeSpawn_Descent</c> at $A5:EB1B.</summary>
    private const ushort SpawnDescentFunction = 0xeb1b;
    /// <summary><c>Function_SporeSpawn_Moving</c> at $A5:EB52.</summary>
    private const ushort SpawnMovingFunction = 0xeb52;
    /// <summary><c>Function_SporeSpawn_SetupDeath</c> at $A5:EB9B.</summary>
    private const ushort SpawnSetupDeathFunction = 0xeb9b;
    /// <summary><c>Function_SporeSpawn_Dying</c> at $A5:EBEE.</summary>
    private const ushort SpawnDyingFunction = 0xebee;

    /// <summary>Native program bank $A5.</summary>
    internal const byte Bank = 0xa5;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xe6b9),
        Entry(InitialDead),
        Op(SpawnLoadDeathSequenceTargetPalette, 0x00c0),
        Op(SpawnFunctionInY, EmptyRoutineEB1A),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InitialAlive),
        Frame(256),
        Op(SpawnFunctionInY, SpawnDescentFunction),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(FightStarted),
        Op(SpawnSetMaxXRadiusAndAngleDelta, 0x0040, 0x0001),
        Op(SpawnFunctionInY, SpawnMovingFunction),
        Frame(768),
        Op(SpawnSporeGenerationFlagInY, 0x0001),
        Op(SpawnQueueSFXInYLib2Max6, 0x002c),
        Frame(1),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(7),
        Frame(7),
        Frame(6),
        Frame(1),
        Op(SpawnClearDamagedFlag),
        Op(SpawnFunctionInY, EmptyRoutineEB1A),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0005),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, SpawnOpenAndStop1),
        Entry(CloseAndMove),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(1),
        Op(SpawnFunctionInY, SpawnMovingFunction),
        Op(SpawnSporeGenerationFlagInY, 0x0000),
        Op(SpawnIncreaseMaxXRadius),
        Frame(512),
        Op(SpawnSporeGenerationFlagInY, 0x0001),
        Frame(208),
        Op(CommonEnemyInstructionCodes.Goto, SpawnOpenAndStop0),
        Origin(0xe77d),
        Entry(Death),
        Op(SpawnFunctionInY, SpawnSetupDeathFunction),
        Frame(1),
        Op(SpawnFunctionInY, SpawnDyingFunction),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x000a),
        Frame(1),
        Op(SpawnSpawnDyingExplosion),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0008),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, SpawnDeathSequence1),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(1),
        Op(SpawnHarden),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x000a),
        Op(SpawnSpawnHardeningDustCloud),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0008),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, SpawnDeathSequence2),
        Op(SpawnLoadDeathSequencePalette, 0x0000),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnLoadDeathSequencePalette, 0x0020),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnLoadDeathSequencePalette, 0x0040),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnLoadDeathSequencePalette, 0x0060),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnLoadDeathSequencePalette, 0x0080),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnLoadDeathSequencePalette, 0x00a0),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnLoadDeathSequencePalette, 0x00c0),
        Op(SpawnSpawnHardeningDustCloud),
        Frame(16),
        Op(SpawnCallSporeSpawnDeathItemDropRoutine),
        Op(CommonEnemyInstructionCodes.Sleep));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;

    /// <summary>Number of interleaved presentation words, compiled separately for installed play.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Returns one mechanics definition for cartridge-equivalence verification.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Returns one live spritemap-word address for boundary verification.</summary>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>
    /// Reads one mechanics word and rejects presentation addresses or pointers outside the
    /// translated family. A restored invalid cursor must not silently resume ROM execution.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Spore Spawn instruction mechanics pointer $A5:{address:X4} is not compiled.");

    /// <summary>True when an absolute address names a byte owned by compiled mechanics.</summary>
    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
