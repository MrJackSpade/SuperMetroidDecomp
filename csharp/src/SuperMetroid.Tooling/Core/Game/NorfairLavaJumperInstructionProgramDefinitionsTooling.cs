using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairLavaJumperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairLavaJumperInstructionProgramDefinitions))]
internal abstract class NorfairLavaJumperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
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
