namespace SuperMetroid.Core.Game;

/// <summary>
/// Golden Torizo's initial bank-$AA entry through its first sleep. The initial
/// tile upload uses the shared compiled $814B descriptor/artwork path. The
/// following fall/awakening list and later combat lists have separate owners.
/// </summary>
internal abstract class GoldenTorizoInitialInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_GoldenTorizo_Initial_0</c> at $AA:C9CB.</summary>
    internal const ushort Initial = 0xc9cb;

    /// <summary>The initial extended-spritemap operand at $AA:C9DE.</summary>
    internal const ushort InitialFrameOperand = 0xc9de;

    /// <summary>Golden Torizo's Samus-position wake function at $AA:D5C2.</summary>
    internal const ushort WakeWhenSamusApproaches = 0xd5c2;

    public static int MechanicsWordCount => 7;

    /// <summary>
    /// $AA:C9CB-C9E1 uploads tiles (two-byte opcode plus seven-byte DMA payload),
    /// configures the standing pose and wake callback, then installs one pose and sleeps.
    /// DMA and visual operands remain owned by their existing dedicated catalogs.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index == 0 ? Initial :
            (ushort)(Initial + 9 + (index - 1) * 2 + (index == 6 ? 2 : 0));
        ushort value = index switch
        {
            0 => CommonEnemyInstructionCodes.CopyToVram,
            1 => TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState,
            2 => TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock,
            3 => TorizoInstructionCodes.Instruction_Torizo_FunctionInY,
            4 => WakeWhenSamusApproaches,
            5 => 1,
            _ => CommonEnemyInstructionCodes.Sleep,
        };
        return new(address, value);
    }
    public static int PresentationWordCount => 1;
    public static ushort PresentationWordAddress(int index) => index == 0
        ? InitialFrameOperand
        : throw new ArgumentOutOfRangeException(nameof(index));

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = MechanicsWord(index);
            if (offset == word.Address || offset == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
