using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoFallingLeftInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoFallingLeftInstructionProgramDefinitions))]
internal abstract class TorizoFallingLeftInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoFallingLeftInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => TorizoFallingLeftInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoFallingLeftInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
