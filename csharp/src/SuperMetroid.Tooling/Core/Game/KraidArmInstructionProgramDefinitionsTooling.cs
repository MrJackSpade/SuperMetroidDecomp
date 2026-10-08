using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidArmInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidArmInstructionProgramDefinitions))]
internal abstract class KraidArmInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => KraidArmInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidArmInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => KraidArmInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => KraidArmInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidArmInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidArmInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
