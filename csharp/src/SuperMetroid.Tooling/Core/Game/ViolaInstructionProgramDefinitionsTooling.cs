using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ViolaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ViolaInstructionProgramDefinitions))]
internal abstract class ViolaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => ViolaInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => ViolaInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => ViolaInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
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
