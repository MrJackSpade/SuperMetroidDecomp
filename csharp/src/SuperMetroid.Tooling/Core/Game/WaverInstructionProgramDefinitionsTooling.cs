using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WaverInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WaverInstructionProgramDefinitions))]
internal abstract class WaverInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => WaverInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => WaverInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 10;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index < 2 ? (ushort)(WaverInstructionProgramDefinitions.SteadyFacingLeft + 6 * index + 2)
            : (ushort)(WaverInstructionProgramDefinitions.SpinningFacingLeft + 20 * ((index - 2) / 4) + 2 + 4 * ((index - 2) % 4));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < WaverInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = WaverInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
