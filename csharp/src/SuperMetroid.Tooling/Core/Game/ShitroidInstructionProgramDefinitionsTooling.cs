using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShitroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShitroidInstructionProgramDefinitions))]
internal abstract class ShitroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of fixed timing and control operands in the compiled Shitroid programs.</summary>
    public static int MechanicsWordCount => ShitroidInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Maps a mechanics slot to its instruction address and fixed operand.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The address and cartridge-defined value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => ShitroidInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of sprite-selector operands supplied as Shitroid presentation data.</summary>
    public static int PresentationWordCount => ShitroidInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of one compiled Shitroid sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot.</param>
    /// <returns>The address of the selected presentation word in bank $A9.</returns>
    public static ushort PresentationWordAddress(int index) => ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address is either byte of a compiled mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank address belongs to a fixed mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ShitroidInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ShitroidInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
