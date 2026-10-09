using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WaverInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WaverInstructionProgramDefinitions))]
internal abstract class WaverInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of fixed timing and control operands in the compiled Waver programs.</summary>
    public static int MechanicsWordCount => WaverInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Maps a mechanics slot to its instruction address and fixed operand.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The address and cartridge-defined value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => WaverInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of sprite-selector operands supplied for Waver's steady and spinning poses.</summary>
    public static int PresentationWordCount => 10;

    /// <summary>Gets the address of one steady- or spinning-pose sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot across both facing directions.</param>
    /// <returns>The address of the selected presentation word in bank $A3.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index < 2 ? (ushort)(WaverInstructionProgramDefinitions.SteadyFacingLeft + 6 * index + 2)
            : (ushort)(WaverInstructionProgramDefinitions.SpinningFacingLeft + 20 * ((index - 2) / 4) + 2 + 4 * ((index - 2) % 4));
    }
    /// <summary>Checks whether an address is either byte of a compiled Waver mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank address belongs to a fixed mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < WaverInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = WaverInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
