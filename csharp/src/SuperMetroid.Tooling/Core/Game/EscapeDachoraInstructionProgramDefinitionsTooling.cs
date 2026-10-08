using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EscapeDachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeDachoraInstructionProgramDefinitions))]
internal abstract class EscapeDachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
