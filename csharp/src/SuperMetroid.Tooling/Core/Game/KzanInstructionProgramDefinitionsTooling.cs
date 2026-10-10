using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KzanInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KzanInstructionProgramDefinitions))]
internal abstract class KzanInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Address of the live <c>Spritemap_Kzan</c> operand at $A6:8B2B.</summary>
    internal const ushort PresentationWord = 0x8b2b;
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;
    public static int MechanicsWordCount => 2;
    public static InstructionMechanicsWord MechanicsWord(int index) => index switch
    {
        0 => new(KzanInstructionProgramDefinitions.Idle, 1),
        1 => new(KzanInstructionProgramDefinitions.Idle + 4, (ushort)CommonEnemyInstruction.Sleep),
        _ => throw new IndexOutOfRangeException(),
    };
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        ((ushort)address - KzanInstructionProgramDefinitions.Idle is 0 or 1 or 4 or 5);
}
