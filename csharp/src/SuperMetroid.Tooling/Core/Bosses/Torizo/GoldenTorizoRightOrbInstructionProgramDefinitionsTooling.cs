using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoRightOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoRightOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoRightOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts the orb attack's ten sprite selections independently of its control stream.</summary>
    public static int PresentationWordCount => GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Finds a presentation operand in the $AA:CC99-CCDA orb attack.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Uses bank $AA for the Golden Torizo orb-attack source span.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoRightOrbInstructionProgramDefinitions.Bank;
    /// <summary>Counts authored timing, callback, and movement-control words in the orb attack.</summary>
    public static int MechanicsWordCount => GoldenTorizoRightOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Supplies one compiled orb-attack control word for the exact source audit.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoRightOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests ownership of control bytes while leaving visual-selector bytes editable.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoRightOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
