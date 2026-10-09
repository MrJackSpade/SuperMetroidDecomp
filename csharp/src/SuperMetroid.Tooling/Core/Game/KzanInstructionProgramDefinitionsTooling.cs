using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KzanInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KzanInstructionProgramDefinitions))]
internal abstract class KzanInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Address of the live <c>Spritemap_Kzan</c> operand at $A6:8B2B.</summary>
    internal const ushort PresentationWord = 0x8b2b;

    /// <summary>Exposes the Kzan spritemap operand address to single-operand catalog tooling.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;

    /// <summary>Number of compiled control words in the Kzan idle instruction program.</summary>
    public static int MechanicsWordCount => 2;

    /// <summary>Returns an idle-program control word in native address order.</summary>
    /// <param name="index">Zero-based ordinal: zero selects the timed frame and one selects its sleep instruction.</param>
    /// <returns>The native address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is not zero or one.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => index switch
    {
        0 => new(KzanInstructionProgramDefinitions.Idle, 1),
        1 => new(KzanInstructionProgramDefinitions.Idle + 4, CommonEnemyInstructionCodes.Sleep),
        _ => throw new IndexOutOfRangeException(),
    };
    /// <summary>Identifies bytes belonging to the compiled idle-frame and sleep words, excluding the spritemap operand.</summary>
    /// <param name="address">Full banked address to classify.</param>
    /// <returns><see langword="true"/> for either byte of those words in bank $A6.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        ((ushort)address - KzanInstructionProgramDefinitions.Idle is 0 or 1 or 4 or 5);
}
