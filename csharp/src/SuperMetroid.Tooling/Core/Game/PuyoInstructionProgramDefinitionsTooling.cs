using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PuyoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PuyoInstructionProgramDefinitions))]
internal abstract class PuyoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled timing and control words in the grounded and airborne Puyo programs.</summary>
    public static int MechanicsWordCount => 28;

    /// <summary>Gets the number of live spritemap operands embedded across the Puyo programs.</summary>
    public static int PresentationWordCount => 17;

    /// <summary>Returns a compiled frame duration, loop command, or airborne-pose control word.</summary>
    /// <param name="index">Zero-based index through the grounded loops followed by the airborne pose programs.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 28 compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int pointer;
        if (index < 18)
        {
            int local = index % 6;
            pointer = PuyoInstructionProgramDefinitions.GroundedFast + index / 6 * 20 + (local < 5 ? local * 4 : 18);
        }
        else pointer = PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4 + (index - 18) / 2 * 6 + (index % 2) * 4;
        return new((ushort)pointer, PuyoInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer));
    }

    /// <summary>Returns the bank-$A2 address of a live spritemap operand in a grounded loop or airborne pose.</summary>
    /// <param name="index">Zero-based operand index through the three grounded loops and five airborne poses.</param>
    /// <returns>Address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 17 presentation operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 12 ? PuyoInstructionProgramDefinitions.GroundedFast + index / 4 * 20 + index % 4 * 4 + 2 :
            PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4 + (index - 12) * 6 + 2);
    }

    /// <summary>Checks whether a bank-$A2 byte is compiled timing or control data rather than a spritemap operand.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for a byte of a mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - PuyoInstructionProgramDefinitions.GroundedFast;
        if ((uint)offset < 60) return offset % 20 >= 16 || offset % 20 % 4 < 2;
        offset = (ushort)address - PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4;
        return (uint)offset < 30 && offset % 6 is not (2 or 3);
    }
}
