using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FlyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FlyInstructionProgramDefinitions))]
internal abstract class FlyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timed-frame and loop-control mechanics words in the shared flight program.</summary>
    public static int MechanicsWordCount => FlyInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a timed-frame or loop-control word from the compiled Mellow, Mella, and Memu flight list.</summary>
    /// <param name="index">Zero-based ordinal among the four frame entries and two loop-control entries.</param>
    /// <returns>The bank-relative instruction address and its mechanics value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => FlyInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of frame spritemap operands in the shared flight animation.</summary>
    public static int PresentationWordCount => FlyInstructionProgramDefinitions.FrameCount;

    /// <summary>Maps a flight-frame ordinal to the address of its spritemap operand.</summary>
    /// <param name="index">Zero-based frame index in the four-frame flight loop.</param>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(FlyInstructionProgramDefinitions.Flight + 2 + 4 * index);
    }
    /// <summary>Checks whether a 24-bit address is either byte of a compiled flight mechanics word in bank $A2.</summary>
    /// <param name="address">Address to test against the shared flight instruction list.</param>
    /// <returns><see langword="true"/> when the address identifies a compiled mechanics word byte.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < FlyInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = FlyInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
