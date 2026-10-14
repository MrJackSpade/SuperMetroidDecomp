using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Development-tool members of <see cref="SbugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SbugInstructionProgramDefinitions))]
internal abstract class SbugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds mechanic commands in the compiled Sbug instruction program.</summary>
    public static int MechanicsWordCount => SbugInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes an addressed mechanic word for comparison with native data.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => SbugInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds Sbug's visual operands apart from its mechanic commands.</summary>
    public static int PresentationWordCount => SbugInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one visual operand in Sbug's instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => SbugInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Accepts either byte of a compiled mechanic word in bank A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SbugInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SbugInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
