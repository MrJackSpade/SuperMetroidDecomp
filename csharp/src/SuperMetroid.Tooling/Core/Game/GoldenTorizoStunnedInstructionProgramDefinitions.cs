using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The callable bank-$AA Golden Torizo stunned program at $AA:D193-D1E6.
/// All 28 words are control data; four existing $814B descriptors stream
/// separately installed Torizo tile art, and the list selects no sprite pose.
/// </summary>
internal abstract class GoldenTorizoStunnedInstructionProgramDefinitions : IInstructionProgramCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>InstList_Torizo_Stunned_0</c> at $AA:D193.</summary>
    internal const ushort Start = 0xd193;
    /// <summary><c>InstList_Torizo_Stunned_1</c> at $AA:D1A1.</summary>
    internal const ushort TileLoop = 0xd1a1;
    /// <summary>First byte after the stunned instruction list, $AA:D1E7.</summary>
    internal const ushort End = 0xd1e7;

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort AttackingMovement = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort WalkingMovement = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Provides the bank that contains this native instruction program.</summary>
    static int IDeclaredProgramBank.Bank => Bank;

    /// <summary>
    /// Compiles the stunned entry, four repeated tile-copy operations, timer loop, and
    /// walking continuation; tile artwork remains in the separately installed descriptors.
    /// </summary>
    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd193),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, AttackingMovement),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0018),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0002),
        Entry(TileLoop),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd1ae),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd1bb),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd1c8),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd1d5),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TileLoop),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0010),
        Op(TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        Op(End),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, WalkingMovement),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return));

    /// <summary>Number of compiled instruction and operand words in the stunned program.</summary>
    public static int MechanicsWordCount => Layout.MechanicsWordCount;

    /// <summary>Returns a compiled control word and its native address by ordinal.</summary>
    /// <param name="index">Zero-based ordinal among the program's mechanics words.</param>
    /// <returns>The bank-local address and value of the selected word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Checks whether a full address selects a byte owned by the compiled mechanics layout.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> for a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
