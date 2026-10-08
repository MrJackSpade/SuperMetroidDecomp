using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BullInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BullInstructionProgramDefinitions))]
internal abstract class BullInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => BullInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => BullInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 8;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index < 4 ? BullInstructionProgramDefinitions.Normal : BullInstructionProgramDefinitions.ShotLoop) + 2 + 4 * (index % 4));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < BullInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = BullInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
