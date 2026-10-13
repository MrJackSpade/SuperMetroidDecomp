using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresRidleyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresRidleyProjectileInstructionProgramDefinitions))]
internal abstract class CeresRidleyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The drawing operands across Ceres Ridley's fireball, afterburn, and spawn lists.</summary>
    public static int PresentationWordCount => CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a drawing operand in the projectile program catalog.</summary>
    public static ushort PresentationWordAddress(int index) => CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>
    /// Forty-two timing and control words across the projectile lists; spawn programs
    /// place a callback immediately after their first drawing instruction.
    /// </summary>
    public static int MechanicsWordCount => 42;
    /// <summary>Reads a compiled word from the fireball, afterburn, or one of three spawn lists.</summary>
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
    /// <summary>Recognizes either byte of a compiled projectile instruction in the enemy projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        int bankAddress = (ushort)address;
        return CeresRidleyProjectileInstructionProgramDefinitions.TryRead(bankAddress, out _) || CeresRidleyProjectileInstructionProgramDefinitions.TryRead(bankAddress - 1, out _);
    }
}
