using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EtecoonInstructionProgramDefinitions))]
internal abstract class EtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => EtecoonInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => EtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => EtecoonInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => EtecoonInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = EtecoonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => EtecoonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
