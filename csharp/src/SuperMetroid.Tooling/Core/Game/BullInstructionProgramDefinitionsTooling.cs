using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BullInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BullInstructionProgramDefinitions))]
internal abstract class BullInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => BullInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => BullInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => 8;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index < 4 ? BullInstructionProgramDefinitions.Normal : BullInstructionProgramDefinitions.ShotLoop) + 2 + 4 * (index % 4));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
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
