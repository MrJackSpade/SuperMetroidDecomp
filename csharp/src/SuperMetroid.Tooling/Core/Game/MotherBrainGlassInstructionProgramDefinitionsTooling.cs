using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainGlassInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainGlassInstructionProgramDefinitions))]
internal abstract class MotherBrainGlassInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
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
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - MotherBrainGlassInstructionProgramDefinitions.ShardGroup0;
        if (offset is >= 0 and < (8 * 36))
        {
            int local = offset % 36;
            return local >= 32 || local % 4 < 2;
        }
        int sparkleOffset = (ushort)address - MotherBrainGlassInstructionProgramDefinitions.Sparkle;
        return sparkleOffset >= 0 && sparkleOffset < 18 && sparkleOffset % 4 < 2;
    }
}
