using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PowampSpikeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampSpikeInstructionProgramDefinitions))]
internal abstract class PowampSpikeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => PowampSpikeInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => PowampSpikeInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 6;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(index < 3 ? PowampSpikeInstructionProgramDefinitions.Initial + index * 4 : PowampSpikeInstructionProgramDefinitions.LoopCommand + (index - 3) * 2);
        return new(address, PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (address & 0xffff) - PowampSpikeInstructionProgramDefinitions.Initial;
        return offset >= 0 && PowampSpikeInstructionProgramDefinitions.TryRead((ushort)(PowampSpikeInstructionProgramDefinitions.Initial + (offset & ~1)), out _);
    }
}
