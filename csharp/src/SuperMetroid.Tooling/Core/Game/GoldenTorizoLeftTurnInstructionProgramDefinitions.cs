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
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd1f1),
        Entry(Dodge),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(24),
        Op((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock),
        Op((ushort)CommonEnemyInstruction.Goto, TorizoWalkingLeftRightLegMoving),
        Entry(Turn),
        Op((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY, SimpleMovement),
        Op((ushort)TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag),
        Frame(8));

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
