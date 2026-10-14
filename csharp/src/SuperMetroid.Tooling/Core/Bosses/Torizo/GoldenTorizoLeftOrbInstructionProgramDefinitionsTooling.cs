using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts artwork operands in the left-orb instruction program.</summary>
    public static int PresentationWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the compiled address of an indexed left-orb artwork operand.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Associates the left-orb operand addresses with their declared native bank.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftOrbInstructionProgramDefinitions.Bank;
    /// <summary>Counts the left-orb layout's compiled engine-control words.</summary>
    public static int MechanicsWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Retains both address and value of an indexed left-orb control word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests byte membership in the compiled left-orb control layout.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
