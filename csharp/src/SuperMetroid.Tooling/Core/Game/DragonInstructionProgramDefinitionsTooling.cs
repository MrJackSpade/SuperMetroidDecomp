using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DragonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DragonInstructionProgramDefinitions))]
internal abstract class DragonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => DragonInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => DragonInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => DragonInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => DragonInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DragonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DragonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
