using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Development-tool members of <see cref="BoulderInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BoulderInstructionProgramDefinitions))]
internal abstract class BoulderInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Eight durations and a goto pair in each of the left and right boulder lists.</summary>
    public static int MechanicsWordCount => 20;
    /// <summary>Eight visual operands in each directional boulder list.</summary>
    public static int PresentationWordCount => 16;
    /// <summary>Reads a duration or terminating goto word from a directional list.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 10;
        ushort address = (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 10) + (word < 8 ? 4 * word : 32 + 2 * (word - 8)));
        return new(address, BoulderInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates the visual operand paired with a boulder frame duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 8) + 4 * (index % 8) + 2);
    }
    /// <summary>Classifies the duration and goto bytes within both bank-$A6 lists.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000) return false;
        int offset = (ushort)address - BoulderInstructionProgramDefinitions.Left;
        if ((uint)offset >= 72) return false;
        int stage = offset % 36;
        return stage >= 32 || stage % 4 < 2;
    }
}
