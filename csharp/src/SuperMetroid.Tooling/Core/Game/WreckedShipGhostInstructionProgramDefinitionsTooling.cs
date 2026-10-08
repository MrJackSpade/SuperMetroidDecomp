using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WreckedShipGhostInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WreckedShipGhostInstructionProgramDefinitions))]
internal abstract class WreckedShipGhostInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
