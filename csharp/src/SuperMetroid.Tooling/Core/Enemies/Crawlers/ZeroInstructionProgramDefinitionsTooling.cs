using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Development-tool members of <see cref="ZeroInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZeroInstructionProgramDefinitions))]
internal abstract class ZeroInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts mechanics words translated from Zero enemy instruction programs.</summary>
    public static int MechanicsWordCount => ZeroInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Zero enemy word and its original address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ZeroInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Zero enemy presentation words retained outside compiled behavior.</summary>
    public static int PresentationWordCount => ZeroInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Zero enemy presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => ZeroInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled Zero enemy words in bank $A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ZeroInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ZeroInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
