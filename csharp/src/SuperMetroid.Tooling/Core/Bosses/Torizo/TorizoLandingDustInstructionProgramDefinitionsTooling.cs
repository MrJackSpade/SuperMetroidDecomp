using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="TorizoLandingDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoLandingDustInstructionProgramDefinitions))]
internal abstract class TorizoLandingDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Tracks the eight artwork operands shared by right- and left-foot dust clouds.</summary>
    public static int PresentationWordCount => TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one bank-$86 dust spritemap operand for extraction audits.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Places both dust-cloud programs in the shared enemy-projectile bank $86.</summary>
    static int IDeclaredProgramBank.Bank => TorizoLandingDustInstructionProgramDefinitions.Bank;
    /// <summary>Counts the timed dust poses and terminal controls that remain compiled mechanics.</summary>
    public static int MechanicsWordCount => TorizoLandingDustInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Exposes one dust control word and its native position for verification.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoLandingDustInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Distinguishes dust-control bytes from the eight live spritemap words.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoLandingDustInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
