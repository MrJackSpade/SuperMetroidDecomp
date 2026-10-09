using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SparkInstructionProgramDefinitions))]
internal abstract class SparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled callback, duration, branch, and terminal-instruction words across Spark's four programs.</summary>
    public static int MechanicsWordCount => SparkInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled mechanics word and its address in one of the Spark instruction programs.</summary>
    /// <param name="index">Zero-based index across activation, active, deactivation, and emitter mechanics words.</param>
    /// <returns>The address and expected value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => SparkInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands resolved through installed Spark artwork.</summary>
    public static int PresentationWordCount => SparkInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of an artwork-selection operand in the native Spark instruction lists.</summary>
    /// <param name="index">Zero-based index among activation, active, deactivation, and emitter presentation words.</param>
    /// <returns>The bank-$A8 address of the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => SparkInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address identifies either byte of a compiled Spark mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A8; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SparkInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SparkInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
