using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SharedCrawlerInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SharedCrawlerInstructionProgramDefinitions))]
internal abstract class SharedCrawlerInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words in one surface list, including setup, pose durations, and terminal control.</summary>
    internal const int WordsPerList = 2 + SharedCrawlerInstructionProgramDefinitions.PoseCount + 2;

    /// <summary>Gets the total address/value mechanics words across all compiled surface lists.</summary>
    public static int MechanicsWordCount => SharedCrawlerInstructionProgramDefinitions.SurfaceCount * WordsPerList;

    /// <summary>Gets the number of live presentation operands, one for each pose on each surface.</summary>
    public static int PresentationWordCount => SharedCrawlerInstructionProgramDefinitions.SurfaceCount * SharedCrawlerInstructionProgramDefinitions.PoseCount;

    /// <summary>Returns the compiled mechanics address/value pair at the requested ordinal across all surface lists.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics-word sequence.</param>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int part = index % WordsPerList;
        int offset = part < 2 ? part * 2
            : part < 2 + SharedCrawlerInstructionProgramDefinitions.PoseCount ? SharedCrawlerInstructionProgramDefinitions.SetupBytes + (part - 2) * SharedCrawlerInstructionProgramDefinitions.PoseBytes
            : SharedCrawlerInstructionProgramDefinitions.SetupBytes + SharedCrawlerInstructionProgramDefinitions.PoseCount * SharedCrawlerInstructionProgramDefinitions.PoseBytes + (part - 2 - SharedCrawlerInstructionProgramDefinitions.PoseCount) * 2;
        ushort address = (ushort)(SharedCrawlerInstructionProgramDefinitions.UpsideRight + index / WordsPerList * SharedCrawlerInstructionProgramDefinitions.ListBytes + offset);
        return new(address, SharedCrawlerInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Returns the native address of a pose's live presentation operand.</summary>
    /// <param name="index">Zero-based presentation-operand index across the compiled surface lists.</param>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(SharedCrawlerInstructionProgramDefinitions.UpsideRight + index / SharedCrawlerInstructionProgramDefinitions.PoseCount * SharedCrawlerInstructionProgramDefinitions.ListBytes + SharedCrawlerInstructionProgramDefinitions.SetupBytes + index % SharedCrawlerInstructionProgramDefinitions.PoseCount * SharedCrawlerInstructionProgramDefinitions.PoseBytes + 2);
    }

    /// <summary>Determines whether an address belongs to compiled mechanics data rather than presentation operands.</summary>
    /// <param name="address">The absolute address to classify.</param>
    /// <returns><see langword="true"/> for an address within a compiled list's mechanics words; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        ushort word = (ushort)(address & 0xfffe);
        int relative = word - SharedCrawlerInstructionProgramDefinitions.UpsideRight;
        return (uint)relative < SharedCrawlerInstructionProgramDefinitions.SurfaceCount * SharedCrawlerInstructionProgramDefinitions.ListBytes && !SharedCrawlerInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
