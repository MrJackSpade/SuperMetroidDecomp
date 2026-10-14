using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Development-tool members of <see cref="TourianEntranceStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TourianEntranceStatueInstructionProgramDefinitions))]
internal abstract class TourianEntranceStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The three live entrance-statue enemy lists each stop immediately; their unused poses remain outside this mechanics catalog.</summary>
    public static int MechanicsWordCount => TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes the stop word for the Ridley, Phantoon, or base-decoration statue slot.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => TourianEntranceStatueInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Matches the two bytes of each compiled stop instruction in bank $AA, excluding the unused pose and back-edge gaps.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000)
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
