using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Development-tool members of <see cref="WorkRobotInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WorkRobotInstructionProgramDefinitions))]
internal abstract class WorkRobotInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The Work Robot's powered and unpowered programs contribute 367 control words across their native address range.</summary>
    public static int MechanicsWordCount => 367;
    /// <summary>Counts 227 interleaved Work Robot spritemap operands retained for artwork resolution.</summary>
    public static int PresentationWordCount => 227;
    /// <summary>Walks the Work Robot range in address order and skips tagged visual operands to find compiled control.</summary>
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
    /// <summary>Finds the indexed tagged visual operand among Work Robot's powered and unpowered programs.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        for (int address = WorkRobotInstructionProgramDefinitions.NoPowerNeutral; address < WorkRobotInstructionProgramDefinitions.EndAddress; address += 2)
            if (WorkRobotInstructionProgramDefinitions.ProgramWord((ushort)address) == WorkRobotInstructionProgramDefinitions.PresentationOperand && index-- == 0) return (ushort)address;
        throw new InvalidOperationException("Work Robot presentation-word index is inconsistent.");
    }
    /// <summary>Accepts bank-$A8 Work Robot bytes only when the aligned word is not tagged as presentation data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        ushort bankAddress = (ushort)address;
        if (bankAddress is < WorkRobotInstructionProgramDefinitions.NoPowerNeutral or >= WorkRobotInstructionProgramDefinitions.EndAddress) return false;
        ushort wordAddress = (ushort)(bankAddress - ((bankAddress - WorkRobotInstructionProgramDefinitions.NoPowerNeutral) & 1));
        return WorkRobotInstructionProgramDefinitions.ProgramWord(wordAddress) != WorkRobotInstructionProgramDefinitions.PresentationOperand;
    }
}
