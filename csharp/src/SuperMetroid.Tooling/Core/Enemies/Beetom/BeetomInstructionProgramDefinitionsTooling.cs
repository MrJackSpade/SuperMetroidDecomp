using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>Development-tool members of <see cref="BeetomInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BeetomInstructionProgramDefinitions))]
internal abstract class BeetomInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Left and right Beetom crawling, hopping, and draining lists contribute 48 compiled control words.</summary>
    public static int MechanicsWordCount => 48;
    /// <summary>The two facings retain 32 spritemap selections across Beetom's three actions.</summary>
    public static int PresentationWordCount => 32;
    /// <summary>Maps an index across facing-specific crawl, hop, and drain control without including frame selectors.</summary>
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
    /// <summary>Locates one of sixteen visual operands in each facing's crawl, hop, or drain programs.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int frame = index % 16;
        int offset = frame < 4 ? 4 + 4 * frame : frame < 8 ? BeetomInstructionProgramDefinitions.HopLeft - BeetomInstructionProgramDefinitions.CrawlingLeft + 4 + 4 * (frame - 4) :
            BeetomInstructionProgramDefinitions.DrainingLeft - BeetomInstructionProgramDefinitions.CrawlingLeft + 2 + 4 * (frame - 8) + (frame >= 12 ? 2 : 0);
        return (ushort)(BeetomInstructionProgramDefinitions.CrawlingLeft + BeetomInstructionProgramDefinitions.FacingStride * (index / 16) + offset);
    }
    /// <summary>Checks bank-$A8 Beetom bytes against the source catalog's control-position map.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        int offset = unchecked((ushort)address) - BeetomInstructionProgramDefinitions.CrawlingLeft;
        return (uint)offset < 2 * BeetomInstructionProgramDefinitions.FacingStride && BeetomInstructionProgramDefinitions.IsMechanicsPosition((offset % BeetomInstructionProgramDefinitions.FacingStride) & ~1);
    }
}
