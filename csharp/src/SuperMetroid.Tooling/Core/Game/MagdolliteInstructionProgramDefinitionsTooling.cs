using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MagdolliteInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteInstructionProgramDefinitions))]
internal abstract class MagdolliteInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => MagdolliteInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => MagdolliteInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MagdolliteInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => MagdolliteInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
