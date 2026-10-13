using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ZebetiteInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZebetiteInstructionProgramDefinitions))]
internal abstract class ZebetiteInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Zebetite mechanic words exposed for native source comparison.</summary>
    public static int MechanicsWordCount => ZebetiteInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Supplies an addressed word from the compiled Zebetite mechanics.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ZebetiteInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds Zebetite's visual operands separately from mechanic words.</summary>
    public static int PresentationWordCount => ZebetiteInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand within the native Zebetite program.</summary>
    public static ushort PresentationWordAddress(int index) => ZebetiteInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches both bytes of compiled mechanic words within bank A6.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ZebetiteInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ZebetiteInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
