using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="StokeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(StokeInstructionProgramDefinitions))]
internal abstract class StokeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => StokeInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => StokeInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 12;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int local = index % 6;
        ushort move = index < 6 ? StokeInstructionProgramDefinitions.MovingLeft : StokeInstructionProgramDefinitions.MovingRight;
        ushort attack = index < 6 ? StokeInstructionProgramDefinitions.AttackingLeft : StokeInstructionProgramDefinitions.AttackingRight;
        return (ushort)(local < 4 ? move + 4 + 4 * local : attack + 2 + 8 * (local - 4));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < StokeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = StokeInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
