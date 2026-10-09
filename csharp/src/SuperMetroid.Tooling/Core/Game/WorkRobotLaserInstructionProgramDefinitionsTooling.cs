using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WorkRobotLaserInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WorkRobotLaserInstructionProgramDefinitions))]
internal abstract class WorkRobotLaserInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of frame-selector operands interleaved with the laser program's waits.</summary>
    public static int PresentationWordCount => WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-relative address of a frame-selector operand by its zero-based position.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The address of the selected operand in the shared laser animation program.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of frame-wait, loop-command, and loop-target words in the compiled laser program.</summary>
    public static int MechanicsWordCount => 9;

    /// <summary>Gets a four-tick frame wait or one of the two words that closes the animation loop.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The bank-relative address and value of the selected wait or loop-control word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
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

    /// <summary>Determines whether a projectile-bank address points to either byte of a compiled wait or loop-control word.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> when the address is a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - WorkRobotLaserInstructionProgramDefinitions.Initial;
        return offset >= 0 && offset < 28 && offset % 4 < 2 || offset >= 28 && offset < 32;
    }
}
