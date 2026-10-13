using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonInstructionProgramDefinitions))]
internal abstract class DraygonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Places body, eye, tail, and arm program audits in native bank $A5.</summary>
    static int IDeclaredProgramBank.Bank => DraygonInstructionProgramDefinitions.Bank;
    /// <summary>Counts engine-control words across the linked Draygon actor programs.</summary>
    public static int MechanicsWordCount => DraygonInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Counts frame selectors kept separate from Draygon's compiled timing and callbacks.</summary>
    public static int PresentationWordCount => DraygonInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Exposes a native address and compiled control value for cartridge comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DraygonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Locates a Draygon OAM or BG2 frame selector left in presentation ownership.</summary>
    public static ushort PresentationWordAddress(int index) => DraygonInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Identifies only bytes compiled as Draygon mechanics for the audit boundary.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DraygonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
