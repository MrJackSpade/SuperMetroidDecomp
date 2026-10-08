using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresSteamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresSteamInstructionProgramDefinitions))]
internal abstract class CeresSteamInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => CeresSteamInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresSteamInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => 36;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        int start = CeresSteamInstructionProgramDefinitions.Up + 52 * (index / 9);
        int frame = index % 9;
        return (ushort)(start + (frame < 2 ? 4 + 12 * frame : 22 + 4 * (frame - 2)));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresSteamInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresSteamInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
