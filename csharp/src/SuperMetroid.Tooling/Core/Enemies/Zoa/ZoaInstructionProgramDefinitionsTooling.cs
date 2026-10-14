using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Development-tool members of <see cref="ZoaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZoaInstructionProgramDefinitions))]
internal abstract class ZoaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Accounts for thirteen mechanic words per facing across shooting and rising.</summary>
    public static int MechanicsWordCount => 26;
    /// <summary>Accounts for three shooting and three rising sprite words per facing.</summary>
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
    /// <summary>Excludes sprite-pointer bytes from the two bank A3 facing programs.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        int relative = (SnesAddressMath.OffsetInBank(address)) - ZoaInstructionProgramDefinitions.FacingLeftShooting;
        if ((uint)relative >= 76) return false;
        int offset = relative % 38;
        // Exclude the two presentation bytes in each timed frame. Both bytes of
        // callbacks, durations and loop instructions/targets belong to mechanics.
        return offset < 18 ? offset % 6 < 4
            : offset < 22 || offset >= 34 || (offset - 22) % 4 < 2;
    }
}
