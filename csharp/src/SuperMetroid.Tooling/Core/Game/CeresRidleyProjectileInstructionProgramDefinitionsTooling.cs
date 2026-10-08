using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresRidleyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresRidleyProjectileInstructionProgramDefinitions))]
internal abstract class CeresRidleyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    // Each draw occupies a duration word and a presentation operand. Spawning
    // programs insert their callback immediately after the first draw.
    public static int MechanicsWordCount => 42;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int address;
        if (index < 11)
        {
            address = index switch
            {
                0 => CeresRidleyProjectileInstructionProgramDefinitions.Fireball,
                1 => CeresRidleyProjectileInstructionProgramDefinitions.Fireball + 2,
                2 => CeresRidleyProjectileInstructionProgramDefinitions.Fireball + 6,
                3 => CeresRidleyProjectileInstructionProgramDefinitions.Fireball + 8,
                4 => CeresRidleyProjectileInstructionProgramDefinitions.Fireball + 10,
                < 9 => CeresRidleyProjectileInstructionProgramDefinitions.FireballLoop + (index - 5) * 4,
                _ => CeresRidleyProjectileInstructionProgramDefinitions.FireballLoop + 16 + (index - 9) * 2,
            };
        }
        else if (index < 18)
        {
            int step = index - 11;
            address = CeresRidleyProjectileInstructionProgramDefinitions.AfterburnFinal + (step == 0 ? 0 : step == 6 ? 22 : 2 + (step - 1) * 4);
        }
        else
        {
            int step = (index - 18) % 8;
            address = CeresRidleyProjectileInstructionProgramDefinitions.SpawnProgram((index - 18) / 8) + (step switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                < 7 => 8 + (step - 3) * 4,
                _ => 24,
            });
        }
        return new((ushort)address, CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int bankAddress = (ushort)address;
        return CeresRidleyProjectileInstructionProgramDefinitions.TryRead(bankAddress, out _) || CeresRidleyProjectileInstructionProgramDefinitions.TryRead(bankAddress - 1, out _);
    }
}
