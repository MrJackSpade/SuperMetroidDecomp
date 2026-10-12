using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ViolaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ViolaInstructionProgramDefinitions))]
internal abstract class ViolaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => ViolaInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ViolaInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => ViolaInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
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
