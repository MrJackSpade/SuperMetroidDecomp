using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EnemyPickupInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyPickupInstructionProgramDefinitions))]
internal abstract class EnemyPickupInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => EnemyPickupInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 30;
    /// <summary>Mechanics word.</summary>
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
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return EnemyPickupInstructionProgramDefinitions.TryRead(bankAddress, out _) || EnemyPickupInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
