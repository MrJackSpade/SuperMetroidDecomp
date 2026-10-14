using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Development-tool members of <see cref="EvirInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EvirInstructionProgramDefinitions))]
internal abstract class EvirInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Evir mechanic words available to the native source audit.</summary>
    public static int MechanicsWordCount => EvirInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Supplies an addressed mechanic word from the compiled Evir program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EvirInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds Evir presentation operands independently of mechanic commands.</summary>
    public static int PresentationWordCount => EvirInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a presentation operand in Evir's native address space.</summary>
    public static ushort PresentationWordAddress(int index) => EvirInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled mechanic word in bank A8.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EvirInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EvirInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
