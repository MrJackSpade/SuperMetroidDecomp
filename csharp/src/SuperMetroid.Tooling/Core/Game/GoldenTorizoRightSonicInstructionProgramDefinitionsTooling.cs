using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoRightSonicInstructionProgramDefinitions))]
internal abstract class GoldenTorizoRightSonicInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of presentation-owned sprite selectors interleaved through both attack lists.</summary>
    public static int PresentationWordCount => GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Resolves one selector slot to its bank-local instruction-word address.</summary>
    /// <param name="index">Zero-based position among the sprite-selector operands.</param>
    /// <returns>The bank-$AA address of the selected presentation operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the selector sequence.</exception>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Bank byte containing the native instruction lists used by this catalog.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoRightSonicInstructionProgramDefinitions.Bank;

    /// <summary>Number of timing, control-flow, and callback words retained as compiled mechanics across both lists.</summary>
    public static int MechanicsWordCount => GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Reads one non-presentation word from the compiled attack-list mechanics.</summary>
    /// <param name="index">Zero-based position in the layout's mechanics-word sequence.</param>
    /// <returns>The bank-local address and value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a full SNES address refers to either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $AA; otherwise <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
