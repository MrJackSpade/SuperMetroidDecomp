using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>Development-tool members of <see cref="SkulteraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkulteraInstructionProgramDefinitions))]
internal abstract class SkulteraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Skultera instruction words whose behavior is compiled.</summary>
    public static int MechanicsWordCount => SkulteraInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Skultera mechanics word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => SkulteraInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Skultera presentation operands retained as visual data.</summary>
    public static int PresentationWordCount => SkulteraInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Skultera presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled Skultera word in bank $A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SkulteraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SkulteraInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
