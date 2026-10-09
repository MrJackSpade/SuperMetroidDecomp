using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BombTorizoDroolInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BombTorizoDroolInstructionProgramDefinitions))]
internal abstract class BombTorizoDroolInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of embedded visual operands in the compiled drool programs.</summary>
    public static int PresentationWordCount => BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-local address of a visual operand by its presentation-slot index.</summary>
    /// <param name="index">Zero-based index among the embedded visual operands.</param>
    /// <returns>The bank-local word address for that presentation slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation operand slots.</exception>
    public static ushort PresentationWordAddress(int index) => BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of non-presentation instruction words in the bounded drool programs.</summary>
    public static int MechanicsWordCount => 19;

    /// <summary>Returns a mechanics word in the address order used by tooling probes.</summary>
    /// <param name="index">Zero-based index among compiled mechanics words, excluding visual operands.</param>
    /// <returns>The native address and value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        // Every two-byte word in the bounded program is control except the seven visuals.
        for (ushort address = BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay; address <= BombTorizoDroolInstructionProgramDefinitions.FloorImpact + 14; address += 2)
        {
            if (BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord(address)) continue;
            if (index-- == 0) return new(address, BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(address));
        }
        throw new InvalidOperationException("Drool program word count does not match its layout.");
    }
    /// <summary>Checks whether a bank-$86 byte address belongs to a compiled mechanics word.</summary>
    /// <param name="address">Full 24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for bytes in compiled mechanics words; visual operands and other addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = unchecked((ushort)address) - BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay;
        return (uint)offset < BombTorizoDroolInstructionProgramDefinitions.FloorImpact + 16 - BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay &&
            !BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord((ushort)(BombTorizoDroolInstructionProgramDefinitions.FourFrameDelay + (offset & ~1)));
    }
}
