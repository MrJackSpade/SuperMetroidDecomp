using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ZoaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZoaInstructionProgramDefinitions))]
internal abstract class ZoaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words across Zoa's left- and right-facing instruction lists.</summary>
    public static int MechanicsWordCount => 26;

    /// <summary>Number of interleaved sprite-pointer operands exposed across both facings.</summary>
    public static int PresentationWordCount => 12;
    /// <summary>Enumerates the thirteen mechanics words per facing in native address order.
    /// Shooting has three callback/timer pairs and a loop pair; rising has three
    /// timers and a loop pair. Presentation words stay interleaved and separate.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int start = index < 13 ? ZoaInstructionProgramDefinitions.FacingLeftShooting : ZoaInstructionProgramDefinitions.FacingRightShooting;
        int field = index % 13;
        int offset = field < 8 ? 6 * (field / 2) + 2 * (field % 2)
            : field < 11 ? 22 + 4 * (field - 8) : 34 + 2 * (field - 11);
        ushort address = (ushort)(start + offset);
        return new(address, ZoaInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Native sprite-pointer positions: three shooting frames at six-byte
    /// stride followed by three rising frames at four-byte stride, for each facing.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int start = index < 6 ? ZoaInstructionProgramDefinitions.FacingLeftShooting : ZoaInstructionProgramDefinitions.FacingRightShooting;
        int frame = index % 6;
        return (ushort)(start + (frame < 3 ? 4 + 6 * frame : 24 + 4 * (frame - 3)));
    }
    /// <summary>Checks whether a full bus address refers to a byte owned by an instruction's mechanics data.</summary>
    /// <param name="address">24-bit bank-$A3 address to classify.</param>
    /// <returns><see langword="true"/> for callbacks, timers, or loop-control bytes in either facing; sprite-pointer bytes are excluded.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int relative = (address & 0xffff) - ZoaInstructionProgramDefinitions.FacingLeftShooting;
        if ((uint)relative >= 76) return false;
        int offset = relative % 38;
        // Exclude the two presentation bytes in each timed frame. Both bytes of
        // callbacks, durations and loop instructions/targets belong to mechanics.
        return offset < 18 ? offset % 6 < 4
            : offset < 22 || offset >= 34 || (offset - 22) % 4 < 2;
    }
}
