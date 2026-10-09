using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairPipeBugInstructionProgramDefinitions))]
internal abstract class NorfairPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing and loop-control words across both facing directions.</summary>
    public static int MechanicsWordCount => NorfairPipeBugInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns the address/value pair for one rising, flying, or loop-control mechanics word.</summary>
    /// <param name="index">Zero-based index in the combined left-facing then right-facing mechanics sequence.</param>
    /// <returns>The compiled bank-$B3 address and word value.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairPipeBugInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of extracted sprite-selection operands interleaved in the rising and flying programs.</summary>
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
    /// <summary>Tests whether a full banked address selects either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full SNES address to check; only bank $B3 and mechanics bytes are accepted.</param>
    /// <returns><see langword="true"/> for a byte in the compiled timing or control words, excluding presentation operands.</returns>
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
