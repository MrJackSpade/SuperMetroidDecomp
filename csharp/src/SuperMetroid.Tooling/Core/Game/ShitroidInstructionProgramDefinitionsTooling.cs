using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShitroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShitroidInstructionProgramDefinitions))]
internal abstract class ShitroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => ShitroidInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => ShitroidInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => ShitroidInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ShitroidInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ShitroidInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ShitroidInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
