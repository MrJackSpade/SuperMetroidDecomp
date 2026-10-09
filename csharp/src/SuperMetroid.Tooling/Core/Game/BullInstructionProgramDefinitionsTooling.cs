using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BullInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BullInstructionProgramDefinitions))]
internal abstract class BullInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words compiled for Bull's normal and shot-loop programs.</summary>
    public static int MechanicsWordCount => BullInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns the compiled mechanics address and value for a normal or shot-loop program ordinal.</summary>
    /// <param name="index">Zero-based ordinal in the shared mechanics catalog.</param>
    /// <returns>The instruction address and mechanics value at that ordinal.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => BullInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands across the four normal and four shot-loop frames.</summary>
    public static int PresentationWordCount => 8;

    /// <summary>Maps a presentation operand ordinal to its address in the normal or shot-loop instruction list.</summary>
    /// <param name="index">Zero-based ordinal across four normal frames followed by four shot-loop frames.</param>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index < 4 ? BullInstructionProgramDefinitions.Normal : BullInstructionProgramDefinitions.ShotLoop) + 2 + 4 * (index % 4));
    }
    /// <summary>Checks whether a 24-bit bank-$A8 address refers to either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit address to test against the compiled normal and shot-loop mechanics.</param>
    /// <returns><see langword="true"/> when the address is either byte of a mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < BullInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = BullInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
