using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PlatformInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PlatformInstructionProgramDefinitions))]
internal abstract class PlatformInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled engine-control words across the eight Kamer and Tripper movement loops.</summary>
    public static int MechanicsWordCount => 56;

    /// <summary>Gets the number of spritemap selector operands interleaved in the platform instruction loops.</summary>
    public static int PresentationWordCount => 32;

    /// <summary>Gets a callback, timer, or loop-control word by its ordinal across the eight native programs.</summary>
    /// <param name="index">Zero-based position in the combined mechanics-word sequence.</param>
    /// <returns>The bank-relative address and compiled value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 7;
        int offset = word == 0 ? 0 : word < 5 ? 2 + 4 * (word - 1) : 18 + 2 * (word - 5);
        ushort address = (ushort)(PlatformInstructionProgramDefinitions.KamerMovingLeft + 22 * (index / 7) + offset);
        return new(address, PlatformInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Gets the bank-relative address of an interleaved spritemap selector by its flattened ordinal.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The native address of the selected presentation operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(PlatformInstructionProgramDefinitions.KamerMovingLeft + 22 * (index / 4) + 4 + 4 * (index % 4));
    }

    /// <summary>Determines whether a bank-$A3 address belongs to a compiled callback, timer, or loop-control word.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> for either byte of a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - PlatformInstructionProgramDefinitions.KamerMovingLeft;
        if ((uint)offset >= 176) return false;
        int local = offset % 22;
        return local < 2 || local >= 18 || (local - 2) % 4 < 2;
    }
}
