using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Development-tool members of <see cref="AlcoonFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AlcoonFireballInstructionProgramDefinitions))]
internal abstract class AlcoonFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Limits the fireball's visual operands exposed for the instruction audit.</summary>
    public static int PresentationWordCount => AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand in the fireball's native instruction span.</summary>
    public static ushort PresentationWordAddress(int index) => AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Covers four spaced instruction words followed by two adjacent mechanic words.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Reads the mechanic words at four-byte intervals, then the trailing two-byte interval.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(AlcoonFireballInstructionProgramDefinitions.Initial + (index < 4 ? 4 * index : 16 + 2 * (index - 4)));
        return new(address, AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Accepts mechanic bytes only within the enemy projectile bank and the fireball's 20-byte span.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - AlcoonFireballInstructionProgramDefinitions.Initial;
        return (uint)offset < 20 && (offset >= 16 || offset % 4 < 2);
    }
}
