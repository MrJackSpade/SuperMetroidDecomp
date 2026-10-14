using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Development-tool members of <see cref="RioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RioInstructionProgramDefinitions))]
internal abstract class RioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Rio instruction words compiled as enemy mechanics.</summary>
    public static int MechanicsWordCount => RioInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Rio word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => RioInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Rio presentation operands kept as visual data.</summary>
    public static int PresentationWordCount => RioInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Rio presentation word in its native program.</summary>
    public static ushort PresentationWordAddress(int index) => RioInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled Rio mechanics word in bank $A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < RioInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = RioInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
