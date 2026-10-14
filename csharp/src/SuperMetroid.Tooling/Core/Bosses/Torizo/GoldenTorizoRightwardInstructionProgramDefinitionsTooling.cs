using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoRightwardInstructionProgramDefinitions))]
internal abstract class GoldenTorizoRightwardInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts presentation words in Golden Torizo's rightward instruction programs.</summary>
    public static int PresentationWordCount => GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Returns the native address of an indexed rightward-program presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the native bank used by the rightward programs.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoRightwardInstructionProgramDefinitions.Bank;
    /// <summary>Counts rightward-program words compiled as behavior.</summary>
    public static int MechanicsWordCount => GoldenTorizoRightwardInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Returns a compiled rightward-program word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoRightwardInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a rightward-program byte is covered by compiled mechanics.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoRightwardInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
