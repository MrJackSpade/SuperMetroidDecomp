using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA Golden Torizo's two right-facing sonic-boom attack lists at
/// $AA:CCDB-CDAE. Their 66 timers, callbacks, movement and loop words are
/// immutable mechanics; 40 interleaved sprite selectors are presentation.
/// </summary>
internal abstract class GoldenTorizoRightSonicInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    internal const ushort Start = 0xccdb;
    internal const ushort RightFootForward = 0xcd45;
    internal const ushort End = 0xcdaf;

    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1</c> at $AA:CCE3.</summary>
    private const ushort TorizoSonicBoomsFacingRightLeftFootFwd1 = 0xcce3;
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1</c> at $AA:CD4D.</summary>
    private const ushort TorizoSonicBoomsFacingRightRightFootFwd1 = 0xcd4d;
    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort TorizoMovementAttackingFunction = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort TorizoMovementWalkingFunction = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xccdb),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementAttackingFunction),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0000),
        Frame(1),
        Frame(4),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0001),
        Frame(1),
        Frame(4),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TorizoSonicBoomsFacingRightLeftFootFwd1),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return),
        Entry(RightFootForward),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementAttackingFunction),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0000),
        Frame(1),
        Frame(4),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(1),
        Frame(1),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY, 0x0001),
        Frame(1),
        Frame(4),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TorizoSonicBoomsFacingRightRightFootFwd1),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
        Layout.TryReadMechanicsWord(address, out value);

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
