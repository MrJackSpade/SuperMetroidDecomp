using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CrocomireInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CrocomireInstructionProgramDefinitions))]
internal abstract class CrocomireInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => CrocomireInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => CrocomireInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = CrocomireInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => CrocomireInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
