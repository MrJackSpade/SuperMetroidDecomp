using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleProjectileInstructionProgramDefinitions))]
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount + 2;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleProjectileInstructionProgramDefinitions.Initial + (index < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - NuclearWaffleProjectileInstructionProgramDefinitions.Initial;
        return (uint)offset < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
