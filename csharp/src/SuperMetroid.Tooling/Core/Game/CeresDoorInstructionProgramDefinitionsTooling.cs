using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresDoorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresDoorInstructionProgramDefinitions))]
internal abstract class CeresDoorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of compiled control words, excluding operands used only to locate presentation data.</summary>
    public static int MechanicsWordCount => CeresDoorInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a control word from the compiled Ceres door instruction program.</summary>
    /// <param name="index">Zero-based position of the control word in the compiled program.</param>
    /// <returns>The instruction mechanics word at that position.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresDoorInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of presentation operands extracted from the instruction program.</summary>
    public static int PresentationWordCount => CeresDoorInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the ROM bank containing the compiled Ceres door instruction program.</summary>
    static int IDeclaredProgramBank.Bank => CeresDoorInstructionProgramDefinitions.Bank;

    /// <summary>Gets the ROM address associated with a presentation operand.</summary>
    /// <param name="index">Zero-based position of the operand in the presentation data.</param>
    /// <returns>The operand's address in the declared program bank.</returns>
    public static ushort PresentationWordAddress(int index) => CeresDoorInstructionProgramDefinitions.PresentationWord(index).Address;

    /// <summary>Determines whether an address identifies either byte of a compiled control word in bank A6.</summary>
    /// <param name="address">Full ROM address to check.</param>
    /// <returns><see langword="true"/> when the address is a control-word byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresDoorInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresDoorInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
