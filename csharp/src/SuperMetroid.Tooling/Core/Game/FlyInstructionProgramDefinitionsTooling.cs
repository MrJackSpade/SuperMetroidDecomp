using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FlyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FlyInstructionProgramDefinitions))]
internal abstract class FlyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => FlyInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => FlyInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => FlyInstructionProgramDefinitions.FrameCount;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(FlyInstructionProgramDefinitions.Flight + 2 + 4 * index);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < FlyInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = FlyInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
