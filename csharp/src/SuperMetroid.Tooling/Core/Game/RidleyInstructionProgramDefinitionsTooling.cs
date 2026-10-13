using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RidleyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyInstructionProgramDefinitions))]
internal abstract class RidleyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Uses bank $A6 for both Ceres and Lower Norfair Ridley program audits.</summary>
    static int IDeclaredProgramBank.Bank => RidleyInstructionProgramDefinitions.Bank;
    /// <summary>Counts fixed engine controls shared by the two Ridley encounters.</summary>
    public static int MechanicsWordCount => RidleyInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Counts interleaved extended-spritemap pointers left in presentation data.</summary>
    public static int PresentationWordCount => RidleyInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Pairs a compiled Ridley control word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RidleyInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Locates a live Ridley visual operand without reading its spritemap value.</summary>
    public static ushort PresentationWordAddress(int index) => RidleyInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether an absolute address is among Ridley's compiled mechanic bytes.</summary>
    public static bool IsCompiledMechanicsByte(int address) => RidleyInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
