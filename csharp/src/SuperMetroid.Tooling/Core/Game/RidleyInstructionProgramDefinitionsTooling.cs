using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RidleyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyInstructionProgramDefinitions))]
internal abstract class RidleyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => RidleyInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => RidleyInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => RidleyInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RidleyInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => RidleyInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => RidleyInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
