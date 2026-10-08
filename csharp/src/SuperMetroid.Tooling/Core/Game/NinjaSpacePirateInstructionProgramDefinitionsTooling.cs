using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NinjaSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NinjaSpacePirateInstructionProgramDefinitions))]
internal abstract class NinjaSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
