using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleProjectileInstructionProgramDefinitions))]
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount + 2;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleProjectileInstructionProgramDefinitions.Initial + (index < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - NuclearWaffleProjectileInstructionProgramDefinitions.Initial;
        return (uint)offset < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
