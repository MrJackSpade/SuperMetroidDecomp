using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies;

/// <summary>Development-tool members of <see cref="EnemyDeathInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyDeathInstructionProgramDefinitions))]
internal abstract class EnemyDeathInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds visual operands in the enemy-death projectile sequences.</summary>
    public static int PresentationWordCount => EnemyDeathInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand for comparison with the native death program.</summary>
    public static ushort PresentationWordAddress(int index) => EnemyDeathInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Covers respawn, big and Mini Kraid explosions, normal and small explosions, and contact death.</summary>
    public static int MechanicsWordCount => 66;
    /// <summary>Maps an index across the six death-program sections to an addressed mechanic word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 3) address = EnemyDeathInstructionProgramDefinitions.RespawnTail + (index == 0 ? 0 : 2 * (index + 1));
        else if (index < 15) address = EnemyDeathInstructionProgramDefinitions.BigExplosion + LoopMechanicsOffset(index - 3, 12);
        else if (index < 29) address = EnemyDeathInstructionProgramDefinitions.MiniKraidExplosion + LoopMechanicsOffset(index - 15, 16);
        else if (index < 38) address = EnemyDeathInstructionProgramDefinitions.NormalExplosion + FrameMechanicsOffset(index - 29, 3, 6);
        else if (index < 47) address = EnemyDeathInstructionProgramDefinitions.SmallExplosion + FrameMechanicsOffset(index - 38, 3, 6);
        else address = EnemyDeathInstructionProgramDefinitions.KilledBySamusContact + FrameMechanicsOffset(index - 47, 7, 16);
        return new((ushort)address, EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    /// <summary>Skips the visual word inserted into a looping explosion sequence.</summary>
    internal static int LoopMechanicsOffset(int index, int delayOffset) =>
        index * 2 + (index > delayOffset / 2 ? 2 : 0);
    /// <summary>Accounts for frame visuals and an inserted sound command in an explosion sequence.</summary>
    internal static int FrameMechanicsOffset(int index, int framesBeforeSound, int frameCount) =>
        index <= framesBeforeSound ? 4 * index
        : index <= frameCount ? 4 * (index - 1) + 2
        : 4 * frameCount + 2 + 2 * (index - frameCount - 1);
    /// <summary>Checks either byte of a compiled death mechanic word in the projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == EnemyProjectileCodePointers.BankBase &&
        (EnemyDeathInstructionProgramDefinitions.TryWord(unchecked((ushort)address), out _) || EnemyDeathInstructionProgramDefinitions.TryWord(unchecked((ushort)(address - 1)), out _));
}
