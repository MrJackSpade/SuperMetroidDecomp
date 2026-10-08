using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TourianEntranceStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TourianEntranceStatueInstructionProgramDefinitions))]
internal abstract class TourianEntranceStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => TourianEntranceStatueInstructionProgramDefinitions.MechanicsWord(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = TourianEntranceStatueInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
