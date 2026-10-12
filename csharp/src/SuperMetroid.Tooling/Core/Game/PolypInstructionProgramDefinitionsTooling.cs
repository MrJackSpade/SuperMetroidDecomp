using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypInstructionProgramDefinitions))]
internal abstract class PolypInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => PolypInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PolypInstructionProgramDefinitions.PresentationWord;
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is 0xb51a or 0xb51b or 0xb51e or 0xb51f;
    }
}
