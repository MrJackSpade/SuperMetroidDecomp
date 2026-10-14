using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Development-tool members of <see cref="ViolaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ViolaInstructionProgramDefinitions))]
internal abstract class ViolaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Viola mechanic words used by the source audit.</summary>
    public static int MechanicsWordCount => ViolaInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Provides an addressed mechanic word from the compiled Viola program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ViolaInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds Viola's visual operands apart from its instruction mechanics.</summary>
    public static int PresentationWordCount => ViolaInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand in Viola's native program.</summary>
    public static ushort PresentationWordAddress(int index) => ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Accepts either byte of a compiled mechanic word in bank A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ViolaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ViolaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
