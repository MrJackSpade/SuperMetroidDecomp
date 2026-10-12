using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadSidehopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadSidehopperInstructionProgramDefinitions))]
internal abstract class DeadSidehopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => DeadSidehopperInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DeadSidehopperInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
