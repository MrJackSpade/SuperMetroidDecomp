using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DragonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DragonInstructionProgramDefinitions))]
internal abstract class DragonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of idle, wing-loop, and attack mechanics words exposed by the Dragon catalog.</summary>
    public static int MechanicsWordCount => DragonInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves a position in the compiled Dragon mechanics sequence to its address and value.</summary>
    /// <param name="index">Zero-based position covering the left/right idle and wing programs followed by both attacks.</param>
    /// <returns>The bank-$A2 instruction address and compiled control or duration word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => DragonInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operand words selected by the compiled idle and attack poses.</summary>
    public static int PresentationWordCount => DragonInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the instruction address of one compiled Dragon spritemap operand.</summary>
    /// <param name="index">Zero-based position among the idle and attack pose operands.</param>
    /// <returns>The bank-$A2 address containing the selected spritemap pointer.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation sequence.</exception>
    public static ushort PresentationWordAddress(int index) => DragonInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute address identifies either byte of a compiled Dragon mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address is in bank $A2 and belongs to compiled mechanics; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DragonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DragonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
