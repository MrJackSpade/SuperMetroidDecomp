using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PhantoonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PhantoonProjectileInstructionProgramDefinitions))]
internal abstract class PhantoonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => PhantoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
