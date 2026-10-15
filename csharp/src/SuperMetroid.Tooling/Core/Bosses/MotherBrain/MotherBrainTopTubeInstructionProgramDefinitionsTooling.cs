using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Development-tool members of <see cref="MotherBrainTopTubeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTopTubeInstructionProgramDefinitions))]
internal abstract class MotherBrainTopTubeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts presentation operand words in Mother Brain's top-tube program.</summary>
    public static int PresentationWordCount => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Returns the cartridge address of an indexed top-tube presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the bank containing the top-tube instruction program.</summary>
    static int IDeclaredProgramBank.Bank => MotherBrainTopTubeInstructionProgramDefinitions.Bank;
    /// <summary>Counts top-tube instruction words compiled as mechanics.</summary>
    public static int MechanicsWordCount => MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs a compiled top-tube mechanics word with its original bank address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Distinguishes compiled top-tube mechanics bytes from presentation data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => MotherBrainTopTubeInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
