using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA callable Golden Torizo right-facing, right-foot-forward Chozo-orb
/// attack at $AA:CC99-CCDA. Timers, callbacks and movement are compiled
/// mechanics; its ten sprite selectors remain independently editable art.
/// </summary>
internal abstract class GoldenTorizoRightOrbInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    internal const ushort Start = 0xcc99;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1</c> at $AA:CCB9.</summary>
    private const ushort TorizoSpewChozoOrbFacingLeftRightFootFwd1 = 0xccb9;
    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort TorizoMovementAttackingFunction = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort TorizoMovementWalkingFunction = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xcc99),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementAttackingFunction),
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
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TorizoSpewChozoOrbFacingLeftRightFootFwd1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
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

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
