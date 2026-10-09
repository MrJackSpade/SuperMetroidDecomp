using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BoulderInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BoulderInstructionProgramDefinitions))]
internal abstract class BoulderInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled Boulder mechanics words across the left- and right-facing programs.</summary>
    public static int MechanicsWordCount => 20;

    /// <summary>Number of presentation operands interleaved across both Boulder programs.</summary>
    public static int PresentationWordCount => 16;

    /// <summary>Resolves an ordinal mechanics entry to its address and cartridge-defined instruction value.</summary>
    /// <param name="index">Zero-based index across the left- then right-facing mechanics words.</param>
    /// <returns>The bank-relative address and compiled mechanics value.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 10;
        ushort address = (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 10) + (word < 8 ? 4 * word : 32 + 2 * (word - 8)));
        return new(address, BoulderInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the address of an interleaved left- or right-facing spritemap operand.</summary>
    /// <param name="index">Zero-based index across presentation operands in program order.</param>
    /// <returns>The bank-relative address of the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled operand range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 8) + 4 * (index % 8) + 2);
    }
    /// <summary>Classifies addresses that belong to Boulder instruction words, excluding their presentation operands.</summary>
    /// <param name="address">Full SNES address to test; only the Boulder program's bank and compiled byte positions are accepted.</param>
    /// <returns><see langword="true"/> when the address is a byte of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000) return false;
        int offset = (ushort)address - BoulderInstructionProgramDefinitions.Left;
        if ((uint)offset >= 72) return false;
        int stage = offset % 36;
        return stage >= 32 || stage % 4 < 2;
    }
}
