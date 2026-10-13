using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoJumpBackLeftInstructionProgramDefinitions))]
internal abstract class TorizoJumpBackLeftInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts the jump-back-left artwork operands in the compiled Torizo program.</summary>
    public static int PresentationWordCount => TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Returns the native address of an indexed jump-back-left artwork operand.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Supplies the program bank needed to interpret its operand addresses.</summary>
    static int IDeclaredProgramBank.Bank => TorizoJumpBackLeftInstructionProgramDefinitions.Bank;
    /// <summary>Counts compiled control words independently of artwork slots.</summary>
    public static int MechanicsWordCount => TorizoJumpBackLeftInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs one jump-back-left control word with its compiled native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoJumpBackLeftInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether a byte address belongs to this Torizo control-word layout.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoJumpBackLeftInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
