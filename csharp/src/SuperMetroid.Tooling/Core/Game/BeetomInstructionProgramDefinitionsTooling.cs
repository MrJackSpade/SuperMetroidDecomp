using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BeetomInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BeetomInstructionProgramDefinitions))]
internal abstract class BeetomInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 48;
    public static int PresentationWordCount => 32;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 24;
        int offset;
        if (word < 7)
            offset = word == 0 ? 0 : word < 5 ? 2 + 4 * (word - 1) : 18 + 2 * (word - 5);
        else if (word < 13)
        {
            int hopWord = word - 7;
            offset = BeetomInstructionProgramDefinitions.HopLeft - BeetomInstructionProgramDefinitions.CrawlingLeft + (hopWord == 0 ? 0 : hopWord < 5 ? 2 + 4 * (hopWord - 1) : 18);
        }
        else
        {
            int drainWord = word - 13;
            offset = BeetomInstructionProgramDefinitions.DrainingLeft - BeetomInstructionProgramDefinitions.CrawlingLeft + (drainWord < 4 ? 4 * drainWord : drainWord == 4 ? 16 :
                drainWord < 9 ? 18 + 4 * (drainWord - 5) : 34 + 2 * (drainWord - 9));
        }
        ushort address = (ushort)(BeetomInstructionProgramDefinitions.CrawlingLeft + BeetomInstructionProgramDefinitions.FacingStride * (index / 24) + offset);
        return new(address, BeetomInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int frame = index % 16;
        int offset = frame < 4 ? 4 + 4 * frame : frame < 8 ? BeetomInstructionProgramDefinitions.HopLeft - BeetomInstructionProgramDefinitions.CrawlingLeft + 4 + 4 * (frame - 4) :
            BeetomInstructionProgramDefinitions.DrainingLeft - BeetomInstructionProgramDefinitions.CrawlingLeft + 2 + 4 * (frame - 8) + (frame >= 12 ? 2 : 0);
        return (ushort)(BeetomInstructionProgramDefinitions.CrawlingLeft + BeetomInstructionProgramDefinitions.FacingStride * (index / 16) + offset);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = unchecked((ushort)address) - BeetomInstructionProgramDefinitions.CrawlingLeft;
        return (uint)offset < 2 * BeetomInstructionProgramDefinitions.FacingStride && BeetomInstructionProgramDefinitions.IsMechanicsPosition((offset % BeetomInstructionProgramDefinitions.FacingStride) & ~1);
    }
}
