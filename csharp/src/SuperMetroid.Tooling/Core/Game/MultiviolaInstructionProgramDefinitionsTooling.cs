using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MultiviolaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MultiviolaInstructionProgramDefinitions))]
internal abstract class MultiviolaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds Multiviola mechanic words used in native source audits.</summary>
    public static int MechanicsWordCount => MultiviolaInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Retrieves an addressed word from the compiled Multiviola mechanics.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => MultiviolaInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds Multiviola presentation operands apart from mechanic words.</summary>
    public static int PresentationWordCount => MultiviolaInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one presentation operand in the native instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => MultiviolaInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches both bytes of each compiled mechanic word in bank A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MultiviolaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MultiviolaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
