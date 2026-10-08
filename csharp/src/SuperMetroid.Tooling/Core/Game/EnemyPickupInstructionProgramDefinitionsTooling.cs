using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EnemyPickupInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyPickupInstructionProgramDefinitions))]
internal abstract class EnemyPickupInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => EnemyPickupInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 30;
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
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return EnemyPickupInstructionProgramDefinitions.TryRead(bankAddress, out _) || EnemyPickupInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
