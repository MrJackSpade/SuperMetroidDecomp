using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PhantoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PhantoonInstructionProgramDefinitions))]
internal abstract class PhantoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => PhantoonInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => PhantoonInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => PhantoonInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => PhantoonInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PhantoonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PhantoonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
