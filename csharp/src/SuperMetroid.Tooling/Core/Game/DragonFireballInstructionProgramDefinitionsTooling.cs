using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DragonFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DragonFireballInstructionProgramDefinitions))]
internal abstract class DragonFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of live spritemap operands embedded across the four fireball loops.</summary>
    public static int PresentationWordCount => DragonFireballInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$86 address of a loop's live spritemap operand.</summary>
    /// <param name="index">Zero-based operand index across the rising and falling loops.</param>
    /// <returns>Address of the indexed presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the eight operands.</exception>
    public static ushort PresentationWordAddress(int index) => DragonFireballInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of compiled wait and loop-control words across the four fireball loops.</summary>
    public static int MechanicsWordCount => 16;

    /// <summary>Returns one compiled wait or loop-control word from the four fireball loops.</summary>
    /// <param name="index">Zero-based index into the mechanics-word sequence.</param>
    /// <returns>The address and value of the indexed control word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the sixteen compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int step = index % 4;
        ushort address = (ushort)(DragonFireballInstructionProgramDefinitions.RisingLeft + index / 4 * 12 + (step < 3 ? step * 4 : 10));
        return new(address, DragonFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Checks whether a bank-$86 address is either byte of a compiled wait or loop-control word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for a compiled mechanics-word byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        (DragonFireballInstructionProgramDefinitions.TryRead((ushort)address, out _) || DragonFireballInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
