using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DragonFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DragonFireballInstructionProgramDefinitions))]
internal abstract class DragonFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Dragon fireball words retained as presentation operands.</summary>
    public static int PresentationWordCount => DragonFireballInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed fireball presentation word in its native program.</summary>
    public static ushort PresentationWordAddress(int index) => DragonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Counts the four mechanics words in each of the four directional fireball sequences.</summary>
    public static int MechanicsWordCount => 16;
    /// <summary>Maps a directional fireball step to its non-contiguous native word address and compiled value.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int step = index % 4;
        ushort address = (ushort)(DragonFireballInstructionProgramDefinitions.RisingLeft + index / 4 * 12 + (step < 3 ? step * 4 : 10));
        return new(address, DragonFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks both bytes of a fireball mechanics word within the enemy-projectile code bank.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == EnemyProjectileCodePointers.BankBase &&
        (DragonFireballInstructionProgramDefinitions.TryRead((ushort)address, out _) || DragonFireballInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
