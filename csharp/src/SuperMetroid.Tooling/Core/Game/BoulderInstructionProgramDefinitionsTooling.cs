using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BoulderInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BoulderInstructionProgramDefinitions))]
internal abstract class BoulderInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 20;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 16;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 10;
        ushort address = (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 10) + (word < 8 ? 4 * word : 32 + 2 * (word - 8)));
        return new(address, BoulderInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 8) + 4 * (index % 8) + 2);
    }
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000) return false;
        int offset = (ushort)address - BoulderInstructionProgramDefinitions.Left;
        if ((uint)offset >= 72) return false;
        int stage = offset % 36;
        return stage >= 32 || stage % 4 < 2;
    }
}
