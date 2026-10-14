using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Development-tool members of <see cref="PowampInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampInstructionProgramDefinitions))]
internal abstract class PowampInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Powamp instruction words compiled as mechanics.</summary>
    public static int MechanicsWordCount => PowampInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Powamp mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PowampInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Powamp presentation operands retained as visual data.</summary>
    public static int PresentationWordCount => PowampInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Powamp presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => PowampInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled Powamp word in bank $A8.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PowampInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PowampInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
