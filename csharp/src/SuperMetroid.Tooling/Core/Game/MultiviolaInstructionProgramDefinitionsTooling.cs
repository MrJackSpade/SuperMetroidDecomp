using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MultiviolaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MultiviolaInstructionProgramDefinitions))]
internal abstract class MultiviolaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timing and loop-control words in Multiviola's compiled animation loop.</summary>
    public static int MechanicsWordCount => MultiviolaInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns the address and encoded value for one timed pose or loop-control mechanics slot.</summary>
    /// <param name="index">Zero-based ordinal in the compiled mechanics-word sequence.</param>
    /// <returns>The instruction address and fixed timer or loop operand at that ordinal.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => MultiviolaInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap selector operands for the fourteen timed poses.</summary>
    public static int PresentationWordCount => MultiviolaInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a timed-pose ordinal to the address of its interleaved spritemap selector.</summary>
    /// <param name="index">Zero-based index among the fourteen animated poses.</param>
    public static ushort PresentationWordAddress(int index) => MultiviolaInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address belongs to a compiled mechanics word in bank $A2.</summary>
    /// <param name="address">24-bit address to test against Multiviola's animation program.</param>
    /// <returns><see langword="true"/> when the address identifies either byte of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MultiviolaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MultiviolaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
