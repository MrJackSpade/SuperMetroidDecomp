using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="AlcoonFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AlcoonFireballInstructionProgramDefinitions))]
internal abstract class AlcoonFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(AlcoonFireballInstructionProgramDefinitions.Initial + (index < 4 ? 4 * index : 16 + 2 * (index - 4)));
        return new(address, AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - AlcoonFireballInstructionProgramDefinitions.Initial;
        return (uint)offset < 20 && (offset >= 16 || offset % 4 < 2);
    }
}
