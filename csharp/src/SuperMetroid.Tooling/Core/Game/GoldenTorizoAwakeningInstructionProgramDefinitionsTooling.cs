using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoAwakeningInstructionProgramDefinitions))]
internal abstract class GoldenTorizoAwakeningInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of interleaved extended-spritemap selector words in the awakening stream.</summary>
    public static int PresentationWordCount => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of an interleaved presentation-selector word.</summary>
    /// <param name="index">Zero-based index in the presentation-word sequence.</param>
    /// <returns>The bank-local address of the selected selector word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of compiled mechanics words in the awakening stream, excluding live presentation selectors and upload descriptors.</summary>
    public static int MechanicsWordCount => 69;

    /// <summary>Gets an expected mechanics word and its address in the compiled awakening stream.</summary>
    /// <param name="index">Zero-based index in the stream's mechanics-word sequence.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return GoldenTorizoAwakeningInstructionProgramDefinitions.Select(index, visual: false);
    }

    /// <summary>Checks whether an address identifies either byte of a compiled awakening mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $AA; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
