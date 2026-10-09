using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HopperInstructionProgramDefinitions))]
internal abstract class HopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing, sound, instruction, and flow-control words across hopper programs.</summary>
    public static int MechanicsWordCount => HopperInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a compiled mechanics word from the concatenated hopper program streams.</summary>
    /// <param name="index">Zero-based index among the mechanics words, excluding spritemap operands.</param>
    /// <returns>The native address and value of the selected timing, sound, instruction, or flow-control word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => HopperInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operand words resolved from installed hopper artwork.</summary>
    public static int PresentationWordCount => HopperInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$A3 address of a visual operand in the concatenated program order.</summary>
    /// <param name="index">Zero-based index among the presentation operands.</param>
    /// <returns>The native address whose word value comes from presentation data.</returns>
    public static ushort PresentationWordAddress(int index) => HopperInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a bank-$A3 byte is part of a compiled mechanics word rather than a visual operand.</summary>
    /// <param name="address">Full 24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> when the byte belongs to one of the compiled mechanics words.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < HopperInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = HopperInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
