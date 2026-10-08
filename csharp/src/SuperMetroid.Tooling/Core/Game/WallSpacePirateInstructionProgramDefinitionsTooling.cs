using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WallSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WallSpacePirateInstructionProgramDefinitions))]
internal abstract class WallSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => WallSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => WallSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => WallSpacePirateInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => WallSpacePirateInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = WallSpacePirateInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => WallSpacePirateInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
