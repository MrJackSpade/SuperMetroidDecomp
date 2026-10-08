using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HopperInstructionProgramDefinitions))]
internal abstract class HopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => HopperInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => HopperInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => HopperInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => HopperInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < HopperInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = HopperInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
