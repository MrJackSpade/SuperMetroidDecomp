using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Development-tool members of <see cref="DeadSidehopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadSidehopperInstructionProgramDefinitions))]
internal abstract class DeadSidehopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Interprets the compiled dead-Sidehopper operands in their native program bank.</summary>
    static int IDeclaredProgramBank.Bank => DeadSidehopperInstructionProgramDefinitions.Bank;
    /// <summary>Counts the compiled control words kept separate from visual operands.</summary>
    public static int MechanicsWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Counts artwork operand slots in the dead-Sidehopper instruction layout.</summary>
    public static int PresentationWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Preserves an indexed control word's native address alongside its value.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Finds the compiled address of an indexed artwork operand slot.</summary>
    public static ushort PresentationWordAddress(int index) => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Tests byte membership in this program's closed mechanics layout.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DeadSidehopperInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
