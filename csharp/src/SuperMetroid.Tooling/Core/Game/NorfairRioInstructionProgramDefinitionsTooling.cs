using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairRioInstructionProgramDefinitions))]
internal abstract class NorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled mechanics words in the Norfair Rio instruction program.</summary>
    public static int MechanicsWordCount => NorfairRioInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a Norfair Rio mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairRioInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Norfair Rio presentation operands left for the artwork catalog.</summary>
    public static int PresentationWordCount => NorfairRioInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Norfair Rio presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled mechanics word in Norfair Rio's bank $A2 program.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairRioInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairRioInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
