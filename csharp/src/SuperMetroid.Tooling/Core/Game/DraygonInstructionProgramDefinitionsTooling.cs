using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonInstructionProgramDefinitions))]
internal abstract class DraygonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => DraygonInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => DraygonInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => DraygonInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DraygonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => DraygonInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DraygonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
