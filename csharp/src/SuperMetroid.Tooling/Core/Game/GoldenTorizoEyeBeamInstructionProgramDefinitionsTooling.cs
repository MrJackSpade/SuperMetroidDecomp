using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoEyeBeamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEyeBeamInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEyeBeamInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
