using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RipperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RipperInstructionProgramDefinitions))]
internal abstract class RipperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of duration and loop-control words across the six family-and-direction programs.</summary>
    public static int MechanicsWordCount => 36;

    /// <summary>Number of interleaved spritemap operands resolved by the installed visual catalog.</summary>
    public static int PresentationWordCount => 24;

    /// <summary>Resolves a mechanics ordinal to its native address and visual-hold or loop-control value.</summary>
    /// <param name="index">Zero-based index across the GRipper, Ripper II, and Ripper programs in both directions.</param>
    /// <returns>The bank-local address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort start = RipperInstructionProgramDefinitions.ProgramStart(index / 6);
        int record = index % 6;
        int offset = record < 4 ? record * 4 : 16 + 2 * (record - 4);
        ushort value = record < 4 ? RipperInstructionProgramDefinitions.VisualHold(record) :
            record == 4 ? CommonEnemyInstructionCodes.Goto : start;
        return new((ushort)(start + offset), value);
    }
    /// <summary>Returns the native address of a spritemap operand by its position among the six programs.</summary>
    /// <param name="index">Zero-based index among the 24 presentation operands.</param>
    /// <returns>The bank-local address of the selected visual operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(RipperInstructionProgramDefinitions.ProgramStart(index / 4) + 4 * (index % 4) + 2);
    }
    /// <summary>Checks whether a full address selects either byte of a compiled animation or loop-control word.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A2; spritemap operands and unrelated addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return RipperInstructionProgramDefinitions.TryRead(bankAddress, out _) || RipperInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
