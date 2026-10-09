using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TourianEntranceStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TourianEntranceStatueInstructionProgramDefinitions))]
internal abstract class TourianEntranceStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of live stop-script words in the three entrance-statue lists.</summary>
    public static int MechanicsWordCount => TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a live stop-script word in the core instruction catalog's native order.</summary>
    /// <param name="index">Zero-based list index, from 0 through <see cref="MechanicsWordCount"/> minus one.</param>
    public static InstructionMechanicsWord MechanicsWord(int index) => TourianEntranceStatueInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Reports whether a full address names either byte of a compiled entrance-statue stop-script word.</summary>
    /// <param name="address">The full SNES address to test; only bank $AA can be owned by these definitions.</param>
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
