using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PowampSpikeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampSpikeInstructionProgramDefinitions))]
internal abstract class PowampSpikeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of looping spike spritemap operands supplied by presentation art.</summary>
    public static int PresentationWordCount => PowampSpikeInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$86 address of a visual operand in the looping spike animation.</summary>
    /// <param name="index">Zero-based index among the three drawing operands.</param>
    /// <returns>The address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the three presentation slots.</exception>
    public static ushort PresentationWordAddress(int index) => PowampSpikeInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of timer, loop-control, and delete words in the compiled spike programs.</summary>
    public static int MechanicsWordCount => 6;

    /// <summary>Returns a mechanics word in address order, including the separate delete-list opcode.</summary>
    /// <param name="index">Zero-based index among the six compiled mechanics words.</param>
    /// <returns>The bank-$86 address and value of the selected timer or control-flow word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(index < 3 ? PowampSpikeInstructionProgramDefinitions.Initial + index * 4 : PowampSpikeInstructionProgramDefinitions.LoopCommand + (index - 3) * 2);
        return new(address, PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether a full cartridge address refers to either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for bank-$86 mechanics bytes; presentation operands and other addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (address & 0xffff) - PowampSpikeInstructionProgramDefinitions.Initial;
        return offset >= 0 && PowampSpikeInstructionProgramDefinitions.TryRead((ushort)(PowampSpikeInstructionProgramDefinitions.Initial + (offset & ~1)), out _);
    }
}
