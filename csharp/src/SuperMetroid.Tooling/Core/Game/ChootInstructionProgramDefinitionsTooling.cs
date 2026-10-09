using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ChootInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChootInstructionProgramDefinitions))]
internal abstract class ChootInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of non-visual instruction words compiled for Choot's idle, jump, and fall programs.</summary>
    public static int MechanicsWordCount => 11;

    /// <summary>Number of instruction operands supplied separately as Choot presentation data.</summary>
    public static int PresentationWordCount => 5;

    /// <summary>Returns the indexed mechanics word across the idle, jumping, and falling programs.</summary>
    /// <param name="index">Zero-based index that excludes the presentation operands.</param>
    /// <returns>The native bank-$A2 address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the eleven mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address;
        if (index < 3)
            address = (ushort)(ChootInstructionProgramDefinitions.Idle + (index == 2 ? 6 : index * 2));
        else
        {
            int frame = (index - 3) % 4;
            int program = (index - 3) / 4;
            address = (ushort)(ChootInstructionProgramDefinitions.Jumping + (ChootInstructionProgramDefinitions.Falling - ChootInstructionProgramDefinitions.Jumping) * program + (frame == 0 ? 0 : frame * 4 - 2));
        }
        return new(address, ChootInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the native address of a Choot operand whose value is supplied by presentation data.</summary>
    /// <param name="index">Zero-based index among the idle, jump, and fall visual operands.</param>
    /// <returns>The bank-$A2 address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the five presentation slots.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index == 0 ? (ushort)(ChootInstructionProgramDefinitions.Idle + 4) :
            (ushort)(ChootInstructionProgramDefinitions.Jumping + (ChootInstructionProgramDefinitions.Falling - ChootInstructionProgramDefinitions.Jumping) * ((index - 1) / 2) + 4 + 4 * ((index - 1) % 2));
    }
    /// <summary>Checks whether a full cartridge address refers to either byte of a compiled Choot mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for a byte in bank $A2 that belongs to a mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return ChootInstructionProgramDefinitions.TryRead(bankAddress, out _) || ChootInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
