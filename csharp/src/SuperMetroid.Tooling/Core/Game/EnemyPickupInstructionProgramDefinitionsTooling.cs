using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EnemyPickupInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyPickupInstructionProgramDefinitions))]
internal abstract class EnemyPickupInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Total number of frame spritemap operands across all five pickup animation programs.</summary>
    public static int PresentationWordCount => EnemyPickupInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-relative spritemap operand address for a flattened pickup-frame index.</summary>
    /// <param name="index">Zero-based ordinal across the presentation frames in all five pickup programs.</param>
    public static ushort PresentationWordAddress(int index) => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of compiled duration and loop-control words across all five pickup programs.</summary>
    public static int MechanicsWordCount => 30;

    /// <summary>Returns the compiled mechanics address and value at its flattened catalog ordinal.</summary>
    /// <param name="index">Zero-based ordinal across the mechanics words in the five pickup loops.</param>
    /// <returns>The instruction address and duration, pointer, or opcode value at that ordinal.</returns>
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
    /// <summary>Checks whether a byte address falls within either byte of a compiled pickup mechanics word.</summary>
    /// <param name="address">24-bit address to test against the bank-$86 pickup programs.</param>
    /// <returns><see langword="true"/> when the address identifies a compiled mechanics word byte.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return EnemyPickupInstructionProgramDefinitions.TryRead(bankAddress, out _) || EnemyPickupInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
