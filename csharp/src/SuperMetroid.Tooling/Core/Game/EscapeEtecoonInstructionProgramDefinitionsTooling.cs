using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EscapeEtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeEtecoonInstructionProgramDefinitions))]
internal abstract class EscapeEtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
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
