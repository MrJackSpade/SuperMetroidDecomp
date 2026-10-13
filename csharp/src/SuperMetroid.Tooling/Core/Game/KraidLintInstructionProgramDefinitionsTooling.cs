using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidLintInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidLintInstructionProgramDefinitions))]
internal abstract class KraidLintInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds Kraid lint mechanic words exposed for native source comparison.</summary>
    public static int MechanicsWordCount => KraidLintInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Supplies an addressed mechanic word from the compiled lint program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidLintInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds the lint's visual operands separately from instruction mechanics.</summary>
    public static int PresentationWordCount => KraidLintInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a presentation operand within Kraid lint's native program.</summary>
    public static ushort PresentationWordAddress(int index) => KraidLintInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches both bytes of compiled mechanic words within bank A7.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidLintInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidLintInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
