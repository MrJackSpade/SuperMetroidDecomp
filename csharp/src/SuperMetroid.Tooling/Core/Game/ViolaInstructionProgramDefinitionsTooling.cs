using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ViolaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ViolaInstructionProgramDefinitions))]
internal abstract class ViolaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words in the four surface entries and their shared normal loop.</summary>
    public static int MechanicsWordCount => ViolaInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled axis-setting, timing, or control-flow word and its instruction address.</summary>
    /// <param name="index">Zero-based position in address order across the surface entries and normal loop.</param>
    /// <returns>The address and expected value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => ViolaInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved spritemap-selector operands in the shared normal loop.</summary>
    public static int PresentationWordCount => ViolaInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of a spritemap-selector operand in the shared normal loop.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The bank-local address of the selected selector word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => ViolaInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address identifies either byte of a compiled Viola mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A3; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ViolaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ViolaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
