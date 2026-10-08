using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoEggInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEggInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEggInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if (TorizoChozoOrbInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) &&
            unchecked((ushort)address) is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab40)
        {
            return true;
        }
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
