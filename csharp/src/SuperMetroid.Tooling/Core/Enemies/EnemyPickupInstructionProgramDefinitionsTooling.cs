using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies;

/// <summary>Development-tool members of <see cref="EnemyPickupInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyPickupInstructionProgramDefinitions))]
internal abstract class EnemyPickupInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts presentation operands across the enemy pickup instruction loops.</summary>
    public static int PresentationWordCount => EnemyPickupInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed pickup-loop presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Counts the compiled mechanics words across all five pickup loops.</summary>
    public static int MechanicsWordCount => 30;
    /// <summary>Maps a flat mechanics index through the five pickup loops to its native word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 5; program++)
        {
            EnemyPickupInstructionProgramDefinitions.PickupLoop loop = EnemyPickupInstructionProgramDefinitions.ProgramAt(program);
            if (index < loop.MechanicsWords)
                return loop.Word(index);
            index -= loop.MechanicsWords;
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Checks either byte of a compiled pickup-loop word in the enemy-projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return EnemyPickupInstructionProgramDefinitions.TryRead(bankAddress, out _) || EnemyPickupInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
