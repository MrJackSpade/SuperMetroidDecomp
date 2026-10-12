using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FuneNamiheInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FuneNamiheInstructionProgramDefinitions))]
internal abstract class FuneNamiheInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Fune and Namihe's left/right idle and opening programs contribute 62 control words.</summary>
    public static int MechanicsWordCount => 62;
    /// <summary>The two species and facings retain 38 interleaved visual operands.</summary>
    public static int PresentationWordCount => 38;
    /// <summary>Walks Fune and Namihe control in species and facing order, skipping frame selectors.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool namihe = index >= 30;
        int wordsPerFacing = namihe ? 16 : 15;
        int speciesIndex = namihe ? index - 30 : index;
        int word = speciesIndex % wordsPerFacing;
        int openingFrames = namihe ? 5 : 4;
        int offset;
        if (word < 2) offset = word * 4; // Idle frame and sleep.
        else if (word < openingFrames + 2) offset = 6 + 4 * (word - 2);
        else if (word < openingFrames + 4) offset = 6 + 4 * openingFrames + 2 * (word - openingFrames - 2);
        else if (word < openingFrames + 8) offset = 10 + 4 * openingFrames + 4 * (word - openingFrames - 4);
        else offset = 26 + 4 * openingFrames + 2 * (word - openingFrames - 8);
        ushort address = (ushort)((namihe ? FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft : FuneNamiheInstructionProgramDefinitions.FuneIdleLeft) +
            (namihe ? 52 : 48) * (speciesIndex / wordsPerFacing) + offset);
        return new(address, FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates an idle, opening, or closing pose selector in Fune or Namihe's bank-$A8 lists.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        bool namihe = index >= 18;
        int frames = namihe ? 10 : 9;
        int speciesIndex = namihe ? index - 18 : index;
        int frame = speciesIndex % frames;
        int offset = frame == 0 ? 2 : 4 + 4 * frame + (frame > (namihe ? 5 : 4) ? 4 : 0);
        return (ushort)((namihe ? FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft : FuneNamiheInstructionProgramDefinitions.FuneIdleLeft) +
            (namihe ? 52 : 48) * (speciesIndex / frames) + offset);
    }
    /// <summary>Aligns words relative to each odd-addressed program start and excludes tagged presentation operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        ushort bankAddress = unchecked((ushort)address);
        if (!FuneNamiheInstructionProgramDefinitions.TryLocate(bankAddress, out _, out _, out int local)) return false;
        ushort word = (ushort)(bankAddress - (local & 1));
        return !FuneNamiheInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
