using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypInstructionProgramDefinitions))]
internal abstract class PolypInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Number of bank-$A2 control words exposed by the compiled Polyp program.</summary>
    public static int MechanicsWordCount => PolypInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads an indexed compiled control word with its bank-relative address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>The single instruction operand that selects Polyp artwork.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PolypInstructionProgramDefinitions.PresentationWord;
    /// <summary>Accepts only the four compiled byte positions at $A2:B51A/B51B/B51E/B51F.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is 0xb51a or 0xb51b or 0xb51e or 0xb51f;
    }
}
