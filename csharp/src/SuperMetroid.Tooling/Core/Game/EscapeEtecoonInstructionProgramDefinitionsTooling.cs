using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EscapeEtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeEtecoonInstructionProgramDefinitions))]
internal abstract class EscapeEtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
