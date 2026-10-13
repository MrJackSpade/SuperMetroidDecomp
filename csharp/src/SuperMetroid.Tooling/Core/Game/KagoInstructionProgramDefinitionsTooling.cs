using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KagoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KagoInstructionProgramDefinitions))]
internal abstract class KagoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Kago instruction words translated into mechanics.</summary>
    public static int MechanicsWordCount => KagoInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Kago mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KagoInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts four presentation frames in each of Kago's slow and fast programs.</summary>
    public static int PresentationWordCount => 8;
    /// <summary>Locates a visual operand within an indexed Kago speed program.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(KagoInstructionProgramDefinitions.Slow + 20 * (index / 4) + 2 + 4 * (index % 4));
    }
    /// <summary>Checks both bytes of each compiled Kago word in bank $A8.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KagoInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KagoInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
