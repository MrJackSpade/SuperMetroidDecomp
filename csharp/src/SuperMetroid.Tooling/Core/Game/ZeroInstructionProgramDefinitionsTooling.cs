using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ZeroInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZeroInstructionProgramDefinitions))]
internal abstract class ZeroInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words across Zero's four orientation-specific surface loops.</summary>
    public static int MechanicsWordCount => ZeroInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an indexed mechanics word to its bank-relative instruction address and encoded value.</summary>
    /// <param name="index">Zero-based index across the four compiled surface loops.</param>
    /// <returns>The instruction address and value for the selected setup, frame, or loop-control word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics table.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => ZeroInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap selections embedded in the four compiled surface loops.</summary>
    public static int PresentationWordCount => ZeroInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of an indexed frame's spritemap operand in its orientation's loop.</summary>
    /// <param name="index">Zero-based ordinal across the six frames of each compiled orientation.</param>
    /// <returns>The bank-relative address containing the spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation table.</exception>
    public static ushort PresentationWordAddress(int index) => ZeroInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word in bank $A3; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ZeroInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ZeroInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
