using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShitroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShitroidInstructionProgramDefinitions))]
internal abstract class ShitroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Baby Metroid's mechanic words exposed to source audits.</summary>
    public static int MechanicsWordCount => ShitroidInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Supplies an addressed mechanic word from the compiled Baby Metroid program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ShitroidInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds presentation operands separately from Baby Metroid mechanics.</summary>
    public static int PresentationWordCount => ShitroidInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand within the native Baby Metroid program.</summary>
    public static ushort PresentationWordAddress(int index) => ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled mechanic word inside bank A9.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ShitroidInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ShitroidInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
