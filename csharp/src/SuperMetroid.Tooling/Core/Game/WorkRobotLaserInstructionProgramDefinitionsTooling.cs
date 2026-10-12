using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WorkRobotLaserInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WorkRobotLaserInstructionProgramDefinitions))]
internal abstract class WorkRobotLaserInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 9;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        return index switch
        {
            7 => new(WorkRobotLaserInstructionProgramDefinitions.LoopCommand, (ushort)EnemyProjectileInstruction.GotoY),
            8 => new(WorkRobotLaserInstructionProgramDefinitions.LoopCommand + 2, WorkRobotLaserInstructionProgramDefinitions.Loop),
            _ => new((ushort)(WorkRobotLaserInstructionProgramDefinitions.Initial + 4 * index), 4),
        };
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - WorkRobotLaserInstructionProgramDefinitions.Initial;
        return offset >= 0 && offset < 28 && offset % 4 < 2 || offset >= 28 && offset < 32;
    }
}
