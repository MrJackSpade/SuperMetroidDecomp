using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WorkRobotInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WorkRobotInstructionProgramDefinitions))]
internal abstract class WorkRobotInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of executable duration and command words reconstructed from the Work Robot program region.</summary>
    public static int MechanicsWordCount => 367;

    /// <summary>Number of pose operands marked for separately installed Work Robot presentation data.</summary>
    public static int PresentationWordCount => 227;

    /// <summary>Resolves the indexed executable word to its instruction address and authored value.</summary>
    /// <param name="index">Zero-based index among the mechanics words, excluding pose operands.</param>
    /// <returns>The bank-local address and value of the selected duration or command word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    /// <exception cref="InvalidOperationException">The reconstructed region does not contain the requested indexed word.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        for (int address = WorkRobotInstructionProgramDefinitions.NoPowerNeutral; address < WorkRobotInstructionProgramDefinitions.EndAddress; address += 2)
        {
            int value = WorkRobotInstructionProgramDefinitions.ProgramWord((ushort)address);
            if (value != WorkRobotInstructionProgramDefinitions.PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Work Robot mechanics-word index is inconsistent.");
    }
    /// <summary>Finds the native address of a marked pose operand in the Work Robot instruction region.</summary>
    /// <param name="index">Zero-based index among the presentation operands.</param>
    /// <returns>The bank-local address of the selected pose word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    /// <exception cref="InvalidOperationException">The reconstructed region does not contain the requested indexed pose word.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        for (int address = WorkRobotInstructionProgramDefinitions.NoPowerNeutral; address < WorkRobotInstructionProgramDefinitions.EndAddress; address += 2)
            if (WorkRobotInstructionProgramDefinitions.ProgramWord((ushort)address) == WorkRobotInstructionProgramDefinitions.PresentationOperand && index-- == 0) return (ushort)address;
        throw new InvalidOperationException("Work Robot presentation-word index is inconsistent.");
    }
    /// <summary>Checks whether an address is a byte of an executable Work Robot word rather than pose data.</summary>
    /// <param name="address">Full 24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A8; pose operands and addresses outside the program region return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort bankAddress = (ushort)address;
        if (bankAddress < WorkRobotInstructionProgramDefinitions.NoPowerNeutral || bankAddress >= WorkRobotInstructionProgramDefinitions.EndAddress) return false;
        ushort wordAddress = (ushort)(bankAddress - ((bankAddress - WorkRobotInstructionProgramDefinitions.NoPowerNeutral) & 1));
        return WorkRobotInstructionProgramDefinitions.ProgramWord(wordAddress) != WorkRobotInstructionProgramDefinitions.PresentationOperand;
    }
}
