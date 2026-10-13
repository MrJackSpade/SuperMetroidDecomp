using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SharedCrawlerInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SharedCrawlerInstructionProgramDefinitions))]
internal abstract class SharedCrawlerInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Two setup words, one duration per pose, and a closing command pair per surface.</summary>
    internal const int WordsPerList = 2 + SharedCrawlerInstructionProgramDefinitions.PoseCount + 2;
    /// <summary>Compiled words across all crawler surface orientations.</summary>
    public static int MechanicsWordCount => SharedCrawlerInstructionProgramDefinitions.SurfaceCount * WordsPerList;
    /// <summary>One drawing operand per pose for every crawler surface orientation.</summary>
    public static int PresentationWordCount => SharedCrawlerInstructionProgramDefinitions.SurfaceCount * SharedCrawlerInstructionProgramDefinitions.PoseCount;
    /// <summary>Locates a setup, duration, or closing word within the selected surface list.</summary>
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
    /// <summary>Locates the drawing operand after a crawler pose duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(SharedCrawlerInstructionProgramDefinitions.UpsideRight + index / SharedCrawlerInstructionProgramDefinitions.PoseCount * SharedCrawlerInstructionProgramDefinitions.ListBytes + SharedCrawlerInstructionProgramDefinitions.SetupBytes + index % SharedCrawlerInstructionProgramDefinitions.PoseCount * SharedCrawlerInstructionProgramDefinitions.PoseBytes + 2);
    }
    /// <summary>Filters bank-$A3 list bytes to exclude the presentation operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        ushort word = (ushort)(address & 0xfffe);
        int relative = word - SharedCrawlerInstructionProgramDefinitions.UpsideRight;
        return (uint)relative < SharedCrawlerInstructionProgramDefinitions.SurfaceCount * SharedCrawlerInstructionProgramDefinitions.ListBytes && !SharedCrawlerInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
