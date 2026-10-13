using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="AtomicInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AtomicInstructionProgramDefinitions))]
internal abstract class AtomicInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Four Atomic direction loops, each with six durations and a terminating goto pair.</summary>
    public static int MechanicsWordCount => 4 * 8;
    /// <summary>Six visual operands in each of the four direction loops.</summary>
    public static int PresentationWordCount => 4 * 6;
    /// <summary>Reads a duration or goto word from the requested direction loop.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        ushort address = (ushort)(AtomicInstructionProgramDefinitions.UpRight + 28 * (index / 8) +
            (word < 6 ? 4 * word : 24 + 2 * (word - 6)));
        return new(address, AtomicInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates the visual operand following a duration in one Atomic loop.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(AtomicInstructionProgramDefinitions.UpRight + 28 * (index / 6) + 4 * (index % 6) + 2);
    }
    /// <summary>Classifies duration and goto bytes in the four bank-$A8 loops.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        int offset = (ushort)address - AtomicInstructionProgramDefinitions.UpRight;
        if ((uint)offset >= 4 * 28) return false;
        int stage = offset % 28;
        return stage >= 24 || stage % 4 < 2;
    }
}
