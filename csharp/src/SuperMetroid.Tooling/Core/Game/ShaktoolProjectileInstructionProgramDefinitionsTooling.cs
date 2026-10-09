using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShaktoolProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolProjectileInstructionProgramDefinitions))]
internal abstract class ShaktoolProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of editable spritemap operands in the three attack-circle programs.</summary>
    public static int PresentationWordCount => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$86 word address of an editable attack-circle spritemap operand.</summary>
    /// <param name="index">Zero-based index among the front, middle, and back program operands.</param>
    /// <returns>Address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the eight operands.</exception>
    public static ushort PresentationWordAddress(int index) => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of compiled command, duration, and control-flow words, excluding presentation operands.</summary>
    public static int MechanicsWordCount => 18;

    /// <summary>Returns one mechanics word from the front, middle, and back attack-circle programs in address order.</summary>
    /// <param name="index">Zero-based index among the compiled mechanics words.</param>
    /// <returns>The address and value of the selected command, duration, or branch word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 18 mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = ShaktoolProjectileInstructionProgramDefinitions.Front; address < ShaktoolProjectileInstructionProgramDefinitions.End; address += 2)
        {
            int value = ShaktoolProjectileInstructionProgramDefinitions.ProgramWord((ushort)address);
            if (value != ShaktoolProjectileInstructionProgramDefinitions.PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Shaktool projectile mechanics-word index is inconsistent.");
    }

    /// <summary>Checks whether a bank-$86 byte belongs to a compiled mechanics word rather than a spritemap operand.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for either byte of a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & 0xfffe);
        return word >= ShaktoolProjectileInstructionProgramDefinitions.Front && word < ShaktoolProjectileInstructionProgramDefinitions.End && ShaktoolProjectileInstructionProgramDefinitions.ProgramWord(word) != ShaktoolProjectileInstructionProgramDefinitions.PresentationOperand;
    }
}
