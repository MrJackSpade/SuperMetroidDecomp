using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainTopTubeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTopTubeInstructionProgramDefinitions))]
internal abstract class MotherBrainTopTubeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the cartridge bank containing this instruction program.</summary>
    static int IDeclaredProgramBank.Bank => MotherBrainTopTubeInstructionProgramDefinitions.Bank;
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => MotherBrainTopTubeInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
