using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KagoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KagoInstructionProgramDefinitions))]
internal abstract class KagoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => KagoInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => KagoInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 8;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(KagoInstructionProgramDefinitions.Slow + 20 * (index / 4) + 2 + 4 * (index % 4));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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
