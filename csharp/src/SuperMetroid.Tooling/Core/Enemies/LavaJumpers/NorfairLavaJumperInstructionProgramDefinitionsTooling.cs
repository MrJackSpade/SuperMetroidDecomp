using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Development-tool members of <see cref="NorfairLavaJumperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairLavaJumperInstructionProgramDefinitions))]
internal abstract class NorfairLavaJumperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Norfair jumper mechanic words used by source audits.</summary>
    public static int MechanicsWordCount => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Retrieves one addressed mechanic word from the compiled jumper program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds visual operands separately from the jumper's mechanic instructions.</summary>
    public static int PresentationWordCount => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a jumper presentation operand in native address space.</summary>
    public static ushort PresentationWordAddress(int index) => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches both bytes of compiled mechanic words in bank A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
