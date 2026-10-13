using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CrocomireInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CrocomireInstructionProgramDefinitions))]
internal abstract class CrocomireInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Identifies the SNES bank used by Crocomire instruction source audits.</summary>
    static int IDeclaredProgramBank.Bank => CrocomireInstructionProgramDefinitions.Bank;
    /// <summary>Bounds the mechanic words exposed by Crocomire's compiled layout.</summary>
    public static int MechanicsWordCount => CrocomireInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Bounds presentation slots separately from mechanic words in the layout.</summary>
    public static int PresentationWordCount => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Pairs a mechanic word's ROM address with its compiled value for source comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = CrocomireInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Locates a presentation slot without interpreting it as a mechanic word.</summary>
    public static ushort PresentationWordAddress(int index) => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Distinguishes compiled mechanic bytes from presentation slots during the audit.</summary>
    public static bool IsCompiledMechanicsByte(int address) => CrocomireInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
