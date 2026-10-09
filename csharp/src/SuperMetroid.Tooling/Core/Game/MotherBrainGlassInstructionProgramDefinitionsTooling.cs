using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainGlassInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainGlassInstructionProgramDefinitions))]
internal abstract class MotherBrainGlassInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of editable shard and sparkle spritemap operands in the presentation layout.</summary>
    public static int PresentationWordCount => MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-relative location of an editable shard or sparkle sprite operand.</summary>
    /// <param name="index">Zero-based presentation slot index.</param>
    /// <returns>Address of the selected spritemap word.</returns>
    public static ushort PresentationWordAddress(int index) => MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of compiled duration and control words across the shard and sparkle programs.</summary>
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
    /// <summary>Tests whether a bank-$86 address points to a compiled duration or control byte.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for mechanics bytes in a shard loop or sparkle program.</returns>
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
