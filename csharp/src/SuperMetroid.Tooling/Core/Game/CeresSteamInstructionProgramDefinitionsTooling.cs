using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresSteamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresSteamInstructionProgramDefinitions))]
internal abstract class CeresSteamInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled instruction words across all four directional steam programs.</summary>
    public static int MechanicsWordCount => CeresSteamInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an ordinal entry in the shared directional control template.</summary>
    /// <param name="index">Zero-based index across the four programs' compiled mechanics words.</param>
    /// <returns>The bank-relative instruction address and its compiled value.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresSteamInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of live spritemap operands interleaved across the four directional programs.</summary>
    public static int PresentationWordCount => 36;

    /// <summary>Maps a presentation ordinal to its cartridge operand in the selected directional program layout.</summary>
    /// <param name="index">Zero-based index among the 36 operands, grouped by direction with nine entries per program.</param>
    /// <returns>The bank-relative address of the live spritemap word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation operand range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        int start = CeresSteamInstructionProgramDefinitions.Up + 52 * (index / 9);
        int frame = index % 9;
        return (ushort)(start + (frame < 2 ? 4 + 12 * frame : 22 + 4 * (frame - 2)));
    }
    /// <summary>Checks whether a full bank-$A6 address belongs to a compiled mechanics word rather than presentation data.</summary>
    /// <param name="address">Full SNES address of the byte to classify.</param>
    /// <returns><see langword="true"/> when the address is either byte of a compiled control word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresSteamInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresSteamInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
