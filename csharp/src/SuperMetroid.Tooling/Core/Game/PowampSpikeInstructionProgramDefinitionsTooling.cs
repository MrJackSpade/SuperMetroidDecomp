using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PowampSpikeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampSpikeInstructionProgramDefinitions))]
internal abstract class PowampSpikeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The extracted drawing operand count in the Powamp spike instruction list.</summary>
    public static int PresentationWordCount => PowampSpikeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Maps a drawing operand index to its bank address for dependency auditing.</summary>
    public static ushort PresentationWordAddress(int index) => PowampSpikeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Three initial mechanics words and three loop-command words are compiled.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Reads a compiled mechanics word from either the initial or loop section.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(index < 3 ? PowampSpikeInstructionProgramDefinitions.Initial + index * 4 : PowampSpikeInstructionProgramDefinitions.LoopCommand + (index - 3) * 2);
        return new(address, PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Accepts bytes in the Powamp spike program that resolve to compiled words in its bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (SnesAddressMath.OffsetInBank(address)) - PowampSpikeInstructionProgramDefinitions.Initial;
        return offset >= 0 && PowampSpikeInstructionProgramDefinitions.TryRead((ushort)(PowampSpikeInstructionProgramDefinitions.Initial + (offset & ~1)), out _);
    }
}
