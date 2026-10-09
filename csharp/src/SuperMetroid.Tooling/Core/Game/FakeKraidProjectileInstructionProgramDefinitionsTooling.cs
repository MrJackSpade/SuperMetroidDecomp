using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FakeKraidProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FakeKraidProjectileInstructionProgramDefinitions))]
internal abstract class FakeKraidProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of sprite-selector operands supplied as presentation data for Fake Kraid's spit poses.</summary>
    public static int PresentationWordCount => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of one spit-pose sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot.</param>
    /// <returns>The address of the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    // Each pose occupies six bytes: duration, visual operand, terminal sleep.
    /// <summary>Number of fixed timing and terminal-control words in the spit program.</summary>
    public static int MechanicsWordCount => 6;

    /// <summary>Maps a mechanics slot to its pose timing or terminal-control word.</summary>
    /// <param name="index">The zero-based slot in the compiled mechanics sequence.</param>
    /// <returns>The instruction address and fixed operand for that slot.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address = (ushort)(FakeKraidProjectileInstructionProgramDefinitions.Spit + 6 * (index / 2) + 4 * (index % 2));
        return new(address, FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether an address is a byte of a compiled duration or terminal-control word.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to one of the fixed mechanics words.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = (ushort)address - FakeKraidProjectileInstructionProgramDefinitions.Spit;
        return (uint)offset < 18 && offset % 6 is 0 or 1 or 4 or 5;
    }
}
