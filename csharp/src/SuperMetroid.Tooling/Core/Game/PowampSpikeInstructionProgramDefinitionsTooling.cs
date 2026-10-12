using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PowampSpikeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampSpikeInstructionProgramDefinitions))]
internal abstract class PowampSpikeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => PowampSpikeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => PowampSpikeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(index < 3 ? PowampSpikeInstructionProgramDefinitions.Initial + index * 4 : PowampSpikeInstructionProgramDefinitions.LoopCommand + (index - 3) * 2);
        return new(address, PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (SnesAddressMath.OffsetInBank(address)) - PowampSpikeInstructionProgramDefinitions.Initial;
        return offset >= 0 && PowampSpikeInstructionProgramDefinitions.TryRead((ushort)(PowampSpikeInstructionProgramDefinitions.Initial + (offset & ~1)), out _);
    }
}
