using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ElevatorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ElevatorInstructionProgramDefinitions))]
internal abstract class ElevatorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of delay and loop-control words in the elevator's compiled animation program.</summary>
    public static int MechanicsWordCount => 4;

    /// <summary>Number of spritemap operands supplied by compiled elevator presentation selectors.</summary>
    public static int PresentationWordCount => 2;

    /// <summary>Returns a mechanics word from the two-frame elevator loop in address order.</summary>
    /// <param name="index">Zero-based index among the loop delays and its terminal goto words.</param>
    /// <returns>The native bank-$A3 address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the four mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(ElevatorInstructionProgramDefinitions.Loop + (index < 2 ? 4 * index : 8 + 2 * (index - 2)));
        return new(address, ElevatorInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the bank-$A3 address of one of the elevator loop's two presentation operands.</summary>
    /// <param name="index">Zero-based frame selector slot.</param>
    /// <returns>The address of the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the two presentation slots.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(ElevatorInstructionProgramDefinitions.Loop + 2 + 4 * index);
    }
    /// <summary>Checks whether a full cartridge address identifies either byte of an elevator mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for compiled mechanics bytes in bank $A3, excluding the presentation operands.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - ElevatorInstructionProgramDefinitions.Loop;
        return (uint)offset < 12 && (offset >= 8 || offset % 4 < 2);
    }
}
