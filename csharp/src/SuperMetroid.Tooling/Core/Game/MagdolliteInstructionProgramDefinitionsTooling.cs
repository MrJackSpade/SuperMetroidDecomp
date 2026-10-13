using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MagdolliteInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteInstructionProgramDefinitions))]
internal abstract class MagdolliteInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Identifies the native bank containing Magdollite instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => MagdolliteInstructionProgramDefinitions.Bank;
    /// <summary>Counts Magdollite instruction words translated into executable mechanics.</summary>
    public static int MechanicsWordCount => MagdolliteInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Counts unresolved presentation slots in the Magdollite program layout.</summary>
    public static int PresentationWordCount => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Pairs a Magdollite mechanics word with its original bank address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MagdolliteInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Locates a Magdollite presentation slot for cartridge operand auditing.</summary>
    public static ushort PresentationWordAddress(int index) => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Tests whether a byte is owned by compiled Magdollite mechanics.</summary>
    public static bool IsCompiledMechanicsByte(int address) => MagdolliteInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
