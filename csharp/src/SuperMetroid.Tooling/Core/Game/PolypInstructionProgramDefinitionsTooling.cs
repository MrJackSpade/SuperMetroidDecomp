using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypInstructionProgramDefinitions))]
internal abstract class PolypInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => PolypInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypInstructionProgramDefinitions.MechanicsWord(index);
    static ushort ISinglePresentationOperand.PresentationWord => PolypInstructionProgramDefinitions.PresentationWord;
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is 0xb51a or 0xb51b or 0xb51e or 0xb51f;
    }
}
