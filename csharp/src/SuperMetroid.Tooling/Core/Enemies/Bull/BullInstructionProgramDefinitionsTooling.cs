using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Development-tool members of <see cref="BullInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BullInstructionProgramDefinitions))]
internal abstract class BullInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Bull instruction words translated into mechanics.</summary>
    public static int MechanicsWordCount => BullInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Bull mechanics word and its original address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => BullInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts four visual frames in each of Bull's normal and shot loops.</summary>
    public static int PresentationWordCount => 8;
    /// <summary>Locates an indexed visual word in the normal or shot Bull loop.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index < 4 ? BullInstructionProgramDefinitions.Normal : BullInstructionProgramDefinitions.ShotLoop) + 2 + 4 * (index % 4));
    }
    /// <summary>Checks both bytes of compiled Bull mechanics words in bank $A8.</summary>
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
