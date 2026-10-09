using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HZoomerInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HZoomerInstructionProgramDefinitions))]
internal abstract class HZoomerInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of initialization, frame-duration, and loop words across the four surface programs.</summary>
    public static int MechanicsWordCount => HZoomerInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves a mechanics ordinal to its native address and compiled control value.</summary>
    /// <param name="index">Zero-based index across the four surface instruction loops.</param>
    /// <returns>The bank-local address and value of the selected engine-control word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => HZoomerInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved spritemap operands resolved from installed presentation artwork.</summary>
    public static int PresentationWordCount => 20;

    /// <summary>Returns the bank-$A3 address of a spritemap operand in one of the four surface loops.</summary>
    /// <param name="index">Zero-based operand index across the four five-frame animations.</param>
    /// <returns>The bank-local instruction address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(HZoomerInstructionProgramDefinitions.UpsideRight + 28 * (index / 5) + 6 + 4 * (index % 5));
    }
    /// <summary>Checks whether a full address selects either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full 24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A3; presentation operands and other addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < HZoomerInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = HZoomerInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
