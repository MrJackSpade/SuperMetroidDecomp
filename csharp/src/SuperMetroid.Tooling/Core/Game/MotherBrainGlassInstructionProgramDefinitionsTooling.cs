using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainGlassInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainGlassInstructionProgramDefinitions))]
internal abstract class MotherBrainGlassInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 85;
    /// <summary>Enumerate ten mechanics words per shard loop, then five sparkle words.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 80)
        {
            int local = index % 10;
            address = MotherBrainGlassInstructionProgramDefinitions.ShardProgram(index / 10) + (local < 8 ? local * 4 : 32 + (local - 8) * 2);
        }
        else address = MotherBrainGlassInstructionProgramDefinitions.Sparkle + (index - 80) * 4;
        return new((ushort)address, MotherBrainGlassInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - MotherBrainGlassInstructionProgramDefinitions.ShardGroup0;
        if (offset >= 0 && offset < 8 * 36)
        {
            int local = offset % 36;
            return local >= 32 || local % 4 < 2;
        }
        int sparkleOffset = (ushort)address - MotherBrainGlassInstructionProgramDefinitions.Sparkle;
        return sparkleOffset >= 0 && sparkleOffset < 18 && sparkleOffset % 4 < 2;
    }
}
