using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Golden Torizo's callable left-foot-forward Chozo-orb attack at
/// $AA:CC57-CC98. Twenty-three control words remain engine-owned while ten
/// interleaved extended-frame selectors are editable presentation.
/// </summary>
internal abstract class GoldenTorizoLeftFootOrbInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0</c> at $AA:CC57.</summary>
    internal const ushort Start = 0xcc57;
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1</c> at $AA:CC77.</summary>
    private const ushort ShotLoop = 0xcc77;
    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort AttackingMovement = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort WalkingMovement = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xcc57),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, AttackingMovement),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0006),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0006),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, ShotLoop),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, WalkingMovement),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
