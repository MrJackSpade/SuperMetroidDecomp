using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YappingMawInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawInstructionProgramDefinitions))]
internal abstract class YappingMawInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => YappingMawInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => YappingMawInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => YappingMawInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < YappingMawInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = YappingMawInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}
