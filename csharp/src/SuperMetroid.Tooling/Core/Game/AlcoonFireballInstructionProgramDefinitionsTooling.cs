using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="AlcoonFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AlcoonFireballInstructionProgramDefinitions))]
internal abstract class AlcoonFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation operands extracted from the compiled Alcoon fireball program.</summary>
    public static int PresentationWordCount => AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the native program address of an extracted presentation operand.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>The address of the operand in the program bank.</returns>
    public static ushort PresentationWordAddress(int index) => AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of instruction words retained as Alcoon fireball mechanics.</summary>
    public static int MechanicsWordCount => 6;

    /// <summary>Gets a compiled mechanics word by its position in the Alcoon fireball program.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The native address and compiled value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(AlcoonFireballInstructionProgramDefinitions.Initial + (index < 4 ? 4 * index : 16 + 2 * (index - 4)));
        return new(address, AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Determines whether an address identifies a byte retained as compiled mechanics rather than presentation data.</summary>
    /// <param name="address">Full bus address to inspect.</param>
    /// <returns><see langword="true"/> for a compiled mechanics byte in the program's bank; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - AlcoonFireballInstructionProgramDefinitions.Initial;
        return (uint)offset < 20 && (offset >= 16 || offset % 4 < 2);
    }
}
