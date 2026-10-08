using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CacatacProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CacatacProjectileInstructionProgramDefinitions))]
internal abstract class CacatacProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => CacatacProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 2 * CacatacProjectileInstructionProgramDefinitions.ProgramCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool sleep = (index & 1) != 0;
        return new((ushort)(CacatacProjectileInstructionProgramDefinitions.LeftFacingUp + 6 * (index / 2) + (sleep ? 4 : 0)),
            sleep ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep : (ushort)1);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0x860000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
