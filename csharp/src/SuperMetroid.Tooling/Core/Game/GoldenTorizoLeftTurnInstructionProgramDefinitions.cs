using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Golden Torizo's callable dodge-turn and ordinary left-turn lists at
/// $AA:D1F1-D20C. Twelve control words stay compiled; both visual operands
/// select the already-editable facing-screen turning frame at $AA:A4F0.
/// </summary>
internal abstract class GoldenTorizoLeftTurnInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningLeft</c> at $AA:D1F1.</summary>
    internal const ushort Dodge = 0xd1f1;
    /// <summary><c>InstList_GoldenTorizo_TurningLeft</c> at $AA:D203.</summary>
    internal const ushort Turn = 0xd203;

    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF.</summary>
    private const ushort SimpleMovement = 0xc6bf;

    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_RightLegMoving</c> at $AA:D20D.</summary>
    private const ushort TorizoWalkingLeftRightLegMoving = 0xd20d;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    /// <summary>Reports the bank containing the dodge-turn and ordinary left-turn instruction streams.</summary>
    static int IDeclaredProgramBank.Bank => Bank;

    /// <summary>Compiled control flow and operand slots for both Golden Torizo left-turn programs.</summary>
    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd1f1),
        Entry(Dodge),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, SimpleMovement),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Op(TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        Op(CommonEnemyInstructionCodes.Goto, TorizoWalkingLeftRightLegMoving),
        Entry(Turn),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, SimpleMovement),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8));

    /// <summary>Number of compiled control words across the dodge-turn and ordinary turn lists.</summary>
    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    /// <summary>Gets one instruction-control word from the compiled bank-$AA programs.</summary>
    /// <param name="index">Zero-based word index in the catalog's mechanics-word ordering.</param>
    /// <returns>The native address and value of the selected control word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Number of visual operands that select the shared left-turn presentation frame.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    /// <summary>Gets the bank-$AA address of one compiled visual-frame operand.</summary>
    /// <param name="index">Zero-based index among the turn programs' presentation operands.</param>
    /// <returns>The address whose value selects the facing-screen turning frame.</returns>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Determines whether an address contains a byte owned by one of the compiled control words.</summary>
    /// <param name="address">Cartridge address tested for compiled mechanics ownership.</param>
    /// <returns><see langword="true"/> when the byte belongs to a translated control word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
