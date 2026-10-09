using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WorkRobotInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WorkRobotInstructionProgramDefinitions))]
internal abstract class WorkRobotInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 367;
    public static int PresentationWordCount => 227;
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
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        for (int address = WorkRobotInstructionProgramDefinitions.NoPowerNeutral; address < WorkRobotInstructionProgramDefinitions.EndAddress; address += 2)
            if (WorkRobotInstructionProgramDefinitions.ProgramWord((ushort)address) == WorkRobotInstructionProgramDefinitions.PresentationOperand && index-- == 0) return (ushort)address;
        throw new InvalidOperationException("Work Robot presentation-word index is inconsistent.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort bankAddress = (ushort)address;
        if (bankAddress is < WorkRobotInstructionProgramDefinitions.NoPowerNeutral or >= WorkRobotInstructionProgramDefinitions.EndAddress) return false;
        ushort wordAddress = (ushort)(bankAddress - ((bankAddress - WorkRobotInstructionProgramDefinitions.NoPowerNeutral) & 1));
        return WorkRobotInstructionProgramDefinitions.ProgramWord(wordAddress) != WorkRobotInstructionProgramDefinitions.PresentationOperand;
    }
}
