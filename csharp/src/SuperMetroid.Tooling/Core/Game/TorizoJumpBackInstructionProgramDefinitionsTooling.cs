using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoJumpBackInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoJumpBackInstructionProgramDefinitions))]
internal abstract class TorizoJumpBackInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts presentation words retained for Torizo's jump-back instruction sequence.</summary>
    public static int PresentationWordCount => TorizoJumpBackInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a jump-back presentation word in the native instruction bank.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoJumpBackInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the bank used to compare the jump-back sequence with the cartridge.</summary>
    static int IDeclaredProgramBank.Bank => TorizoJumpBackInstructionProgramDefinitions.Bank;
    /// <summary>Counts jump-back instruction words whose behavior is compiled into mechanics.</summary>
    public static int MechanicsWordCount => TorizoJumpBackInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Returns a compiled jump-back word alongside the native address it replaces.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoJumpBackInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether a native jump-back byte belongs to the compiled mechanics region.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoJumpBackInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
