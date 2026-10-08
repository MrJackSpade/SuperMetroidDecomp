using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ChozoStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoStatueInstructionProgramDefinitions))]
internal abstract class ChozoStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => ChozoStatueInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoStatueInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => ChozoStatueInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ChozoStatueInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ChozoStatueInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
