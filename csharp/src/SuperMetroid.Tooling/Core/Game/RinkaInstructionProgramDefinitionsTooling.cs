using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RinkaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RinkaInstructionProgramDefinitions))]
internal abstract class RinkaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation operands extracted from the Rinka instruction program.</summary>
    public static int PresentationWordCount => RinkaInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the ROM address associated with a presentation operand.</summary>
    /// <param name="index">Zero-based position of the operand in the presentation data.</param>
    /// <returns>The operand's address in the instruction program.</returns>
    public static ushort PresentationWordAddress(int index) => RinkaInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of mechanics words in one Rinka instruction list.</summary>
    internal const int WordsPerList = 3 + RinkaInstructionProgramDefinitions.PoseCount + 2;

    /// <summary>Gets the total number of mechanics words across the two compiled instruction lists.</summary>
    public static int MechanicsWordCount => 2 * WordsPerList;

    /// <summary>Gets a mechanics word from the two compiled Rinka instruction lists.</summary>
    /// <param name="index">Zero-based position across both lists.</param>
    /// <returns>The word's ROM address and value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int part = index % WordsPerList;
        int offset = part switch
        {
            0 => 0,
            1 => 2,
            2 => 6,
            _ when part < 3 + RinkaInstructionProgramDefinitions.PoseCount => RinkaInstructionProgramDefinitions.SetupBytes + (part - 3) * 4,
            _ => RinkaInstructionProgramDefinitions.SetupBytes + RinkaInstructionProgramDefinitions.PoseCount * 4 + (part - 3 - RinkaInstructionProgramDefinitions.PoseCount) * 2,
        };
        ushort address = (ushort)(RinkaInstructionProgramDefinitions.OrdinaryInitial + index / WordsPerList * RinkaInstructionProgramDefinitions.ListBytes + offset);
        return new(address, RinkaInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Determines whether an address identifies a compiled mechanics byte rather than a presentation operand.</summary>
    /// <param name="address">Full ROM address to check.</param>
    /// <returns><see langword="true"/> for a mechanics byte in the compiled A2-bank lists; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int relative = (address & 0xfffe) - RinkaInstructionProgramDefinitions.OrdinaryInitial;
        return (uint)relative < 2 * RinkaInstructionProgramDefinitions.ListBytes && !RinkaInstructionProgramDefinitions.IsPresentationOffset(relative % RinkaInstructionProgramDefinitions.ListBytes);
    }
}
