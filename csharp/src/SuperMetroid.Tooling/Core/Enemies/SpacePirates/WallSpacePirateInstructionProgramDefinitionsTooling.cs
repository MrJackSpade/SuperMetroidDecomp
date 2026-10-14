using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Development-tool members of <see cref="WallSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WallSpacePirateInstructionProgramDefinitions))]
internal abstract class WallSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts native wall-pirate instruction words retained as presentation operands.</summary>
    public static int PresentationWordCount => WallSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a wall-pirate presentation operand in its native bank.</summary>
    public static ushort PresentationWordAddress(int index) => WallSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the bank containing wall-pirate instruction words.</summary>
    static int IDeclaredProgramBank.Bank => WallSpacePirateInstructionProgramDefinitions.Bank;
    /// <summary>Counts wall-pirate instruction words whose behavior is compiled.</summary>
    public static int MechanicsWordCount => WallSpacePirateInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Returns a compiled wall-pirate mechanics word with its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = WallSpacePirateInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Distinguishes wall-pirate mechanics bytes from presentation operands.</summary>
    public static bool IsCompiledMechanicsByte(int address) => WallSpacePirateInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
