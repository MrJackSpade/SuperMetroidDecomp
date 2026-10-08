using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EyeDoorSweatInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EyeDoorSweatInstructionProgramDefinitions))]
internal abstract class EyeDoorSweatInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
