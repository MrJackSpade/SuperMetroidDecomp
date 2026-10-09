using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KagoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KagoInstructionProgramDefinitions))]
internal abstract class KagoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of fixed timing and control operands in Kago's compiled instruction lists.</summary>
    public static int MechanicsWordCount => KagoInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Maps a mechanics slot to its instruction address and cartridge-defined operand.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The address and fixed value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => KagoInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved sprite-selector words across Kago's two instruction lists.</summary>
    public static int PresentationWordCount => 8;

    /// <summary>Gets the address of a sprite selector in the slow or fast Kago list.</summary>
    /// <param name="index">The zero-based presentation slot across both lists.</param>
    /// <returns>The bank-$A8 address containing the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(KagoInstructionProgramDefinitions.Slow + 20 * (index / 4) + 2 + 4 * (index % 4));
    }
    /// <summary>Checks whether an address is either byte of a compiled Kago mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank address belongs to a fixed mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KagoInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KagoInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
