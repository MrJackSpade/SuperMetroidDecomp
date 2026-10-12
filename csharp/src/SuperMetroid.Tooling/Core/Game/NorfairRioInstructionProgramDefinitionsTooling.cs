using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairRioInstructionProgramDefinitions))]
internal abstract class NorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => NorfairRioInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairRioInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => NorfairRioInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
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
