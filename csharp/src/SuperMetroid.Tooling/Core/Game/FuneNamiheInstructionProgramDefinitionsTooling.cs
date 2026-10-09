using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FuneNamiheInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FuneNamiheInstructionProgramDefinitions))]
internal abstract class FuneNamiheInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled timing, callback, and loop-control words across both species and facings.</summary>
    public static int MechanicsWordCount => 62;

    /// <summary>Gets the number of live spritemap operands interleaved with the eight instruction programs.</summary>
    public static int PresentationWordCount => 38;

    /// <summary>Gets one compiled mechanics word in Fune-then-Namihe program order.</summary>
    /// <param name="index">Zero-based index into the adapter's compiled mechanics-word sequence.</param>
    /// <returns>The address and value of the selected control word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 62 mechanics words.</exception>
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

    /// <summary>Returns the bank-$A8 address of an interleaved visual-frame pointer.</summary>
    /// <param name="index">Zero-based operand index across the Fune and Namihe programs.</param>
    /// <returns>Address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 38 presentation operands.</exception>
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

    /// <summary>Checks whether a bank-$A8 byte belongs to compiled control data rather than a live presentation operand.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for a byte of a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort bankAddress = unchecked((ushort)address);
        if (!FuneNamiheInstructionProgramDefinitions.TryLocate(bankAddress, out _, out _, out int local)) return false;
        // Every program begins at an odd bank address; align relative to that start.
        ushort word = (ushort)(bankAddress - (local & 1));
        return !FuneNamiheInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
