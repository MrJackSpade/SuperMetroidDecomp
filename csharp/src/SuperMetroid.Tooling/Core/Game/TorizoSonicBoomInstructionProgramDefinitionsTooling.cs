using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoSonicBoomInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoSonicBoomInstructionProgramDefinitions))]
internal abstract class TorizoSonicBoomInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoSonicBoomInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => TorizoSonicBoomInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => TorizoSonicBoomInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoSonicBoomInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoSonicBoomInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
