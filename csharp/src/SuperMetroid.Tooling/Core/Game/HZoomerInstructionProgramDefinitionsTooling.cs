using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HZoomerInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HZoomerInstructionProgramDefinitions))]
internal abstract class HZoomerInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => HZoomerInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => HZoomerInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 20;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(HZoomerInstructionProgramDefinitions.UpsideRight + 28 * (index / 5) + 6 + 4 * (index % 5));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < HZoomerInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = HZoomerInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
