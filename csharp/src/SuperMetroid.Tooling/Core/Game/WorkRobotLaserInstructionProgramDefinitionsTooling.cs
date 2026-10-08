using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WorkRobotLaserInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WorkRobotLaserInstructionProgramDefinitions))]
internal abstract class WorkRobotLaserInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 9;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        return index switch
        {
            7 => new(WorkRobotLaserInstructionProgramDefinitions.LoopCommand, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            8 => new(WorkRobotLaserInstructionProgramDefinitions.LoopCommand + 2, WorkRobotLaserInstructionProgramDefinitions.Loop),
            _ => new((ushort)(WorkRobotLaserInstructionProgramDefinitions.Initial + 4 * index), 4),
        };
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - WorkRobotLaserInstructionProgramDefinitions.Initial;
        return offset >= 0 && offset < 28 && offset % 4 < 2 || offset >= 28 && offset < 32;
    }
}
