using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SbugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SbugInstructionProgramDefinitions))]
internal abstract class SbugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words across Sbug's eight directional loops.</summary>
    public static int MechanicsWordCount => SbugInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves one indexed mechanics word to its native instruction address and encoded value.</summary>
    /// <param name="index">Zero-based index across the eight directional programs.</param>
    /// <returns>The instruction address and value for the selected frame or loop-control word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => SbugInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands interleaved with Sbug's directional frame records.</summary>
    public static int PresentationWordCount => SbugInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-relative address of an indexed directional-frame spritemap operand.</summary>
    /// <param name="index">Zero-based index across the presentation operands in all eight loops.</param>
    /// <returns>The native instruction address containing the operand.</returns>
    public static ushort PresentationWordAddress(int index) => SbugInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word in bank $A3; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SbugInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SbugInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
