using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NinjaSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NinjaSpacePirateInstructionProgramDefinitions))]
internal abstract class NinjaSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled instruction, callback, timer, and sound words across the twenty facing-paired programs.</summary>
    public static int MechanicsWordCount => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a mechanics word by its stable ordinal across both facings and all action programs.</summary>
    /// <param name="index">Zero-based ordinal among mechanics words, excluding timed pose words.</param>
    /// <returns>The native bank-$B2 address and compiled value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of timed pose words whose durations are supplied as presentation data.</summary>
    public static int PresentationWordCount => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of one timed pose word retained as presentation data.</summary>
    /// <param name="index">Zero-based ordinal among the pose-duration operands.</param>
    /// <returns>The bank-$B2 address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation sequence.</exception>
    public static ushort PresentationWordAddress(int index) => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a full cartridge address identifies either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for bank-$B2 mechanics bytes; pose-duration presentation words and other addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
