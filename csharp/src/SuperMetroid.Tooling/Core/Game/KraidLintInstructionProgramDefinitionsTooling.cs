using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidLintInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidLintInstructionProgramDefinitions))]
internal abstract class KraidLintInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of duration and terminal-instruction words compiled from Kraid's two lint poses.</summary>
    public static int MechanicsWordCount => KraidLintInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled duration or terminal instruction by its ordinal across the two poses.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The native address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidLintInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of visual operands extracted from Kraid's two lint poses.</summary>
    public static int PresentationWordCount => KraidLintInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-local address of a pose's visual operand.</summary>
    /// <param name="index">Zero-based pose position in the presentation-operand sequence.</param>
    /// <returns>The bank-local address of the operand two bytes into the selected pose.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => KraidLintInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether a bank-$A7 address points to either byte of a compiled lint-pose mechanics word.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> if the address is a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidLintInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidLintInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
