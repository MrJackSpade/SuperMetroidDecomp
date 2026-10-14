using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftFootOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts presentation operand words in the Golden Torizo left-foot orb program.</summary>
    public static int PresentationWordCount => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a left-foot orb presentation operand for native-data comparison.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the bank holding the left-foot orb instruction list.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Bank;
    /// <summary>Counts left-foot orb words translated into compiled mechanics.</summary>
    public static int MechanicsWordCount => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs a compiled left-foot orb instruction word with its original address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether the specified native byte belongs to left-foot orb mechanics.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
