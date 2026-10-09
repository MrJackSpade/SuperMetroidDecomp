using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SkulteraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkulteraInstructionProgramDefinitions))]
internal abstract class SkulteraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled swim, turn, and control mechanics words across Skultera's four programs.</summary>
    public static int MechanicsWordCount => SkulteraInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a compiled mechanics address and value in left-starting-then-right-starting program order.</summary>
    /// <param name="index">Zero-based ordinal among all swimming, turning, and control words.</param>
    /// <returns>The instruction address and encoded duration, callback, or opcode.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => SkulteraInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap-pointer operands across the four compiled swimming and turning programs.</summary>
    public static int PresentationWordCount => SkulteraInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a presentation ordinal to the address of its spritemap-pointer operand.</summary>
    /// <param name="index">Zero-based ordinal among all compiled swimming and turning poses.</param>
    public static ushort PresentationWordAddress(int index) => SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address belongs to a compiled mechanics word in bank $A3.</summary>
    /// <param name="address">24-bit address to test against Skultera's swimming and turning programs.</param>
    /// <returns><see langword="true"/> when the address identifies either byte of a mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SkulteraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SkulteraInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
