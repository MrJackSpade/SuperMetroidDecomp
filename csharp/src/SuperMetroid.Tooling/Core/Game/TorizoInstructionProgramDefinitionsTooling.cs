using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoInstructionProgramDefinitions))]
internal abstract class TorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => TorizoInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => TorizoInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => TorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
