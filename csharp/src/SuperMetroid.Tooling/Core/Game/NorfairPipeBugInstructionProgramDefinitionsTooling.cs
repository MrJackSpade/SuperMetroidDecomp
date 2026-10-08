using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairPipeBugInstructionProgramDefinitions))]
internal abstract class NorfairPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => NorfairPipeBugInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairPipeBugInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 28;
    /// <summary>Each timed record interleaves its visual selector two bytes after duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        int facing = index / 14;
        int frame = index % 14;
        bool flying = frame >= 8;
        if (flying)
            frame -= 8;
        return (ushort)((flying ? NorfairPipeBugInstructionProgramDefinitions.FlyingLeft : NorfairPipeBugInstructionProgramDefinitions.RisingLeft) + 64 * facing + 4 * frame + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairPipeBugInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairPipeBugInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
