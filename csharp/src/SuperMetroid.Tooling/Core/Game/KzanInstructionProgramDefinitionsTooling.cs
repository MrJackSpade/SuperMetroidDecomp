using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KzanInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KzanInstructionProgramDefinitions))]
internal abstract class KzanInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Address of the live <c>Spritemap_Kzan</c> operand at $A6:8B2B.</summary>
    internal const ushort PresentationWord = 0x8b2b;
    /// <summary>Exposes the single compiled Kzan artwork operand through the audit interface.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;
    /// <summary>The idle delay and sleep command are Kzan's only compiled control words.</summary>
    public static int MechanicsWordCount => 2;
    /// <summary>Returns Kzan's idle one-tick delay or sleep command with its $A6 address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => index switch
    {
        0 => new(KzanInstructionProgramDefinitions.Idle, 1),
        1 => new(KzanInstructionProgramDefinitions.Idle + 4, (ushort)CommonEnemyInstruction.Sleep),
        _ => throw new IndexOutOfRangeException(),
    };
    /// <summary>Accepts bytes of the two compiled words at $A6:8B29 and $A6:8B2D only.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xa60000 &&
        ((ushort)address - KzanInstructionProgramDefinitions.Idle is 0 or 1 or 4 or 5);
}
