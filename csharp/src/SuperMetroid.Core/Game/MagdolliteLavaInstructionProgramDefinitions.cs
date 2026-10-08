namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Magdollite's left/right thrown-lava poses and shot program.
/// Interleaved spritemap operands resolve through extracted presentation art; the shot
/// program's final target belongs to the shared projectile-program catalog.
/// </summary>
internal abstract class MagdolliteLavaInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_MagdolliteFlame_Left</c> at $86:DFD8.</summary>
    internal const ushort Left = 0xdfd8;

    /// <summary><c>InstList_EnemyProjectile_MagdolliteFlame_Right</c> at $86:DFDE.</summary>
    internal const ushort Right = 0xdfde;

    /// <summary><c>InstList_EnemyProjectile_Shot_MagdolliteFlame</c> at $86:DFE4.</summary>
    internal const ushort Shot = 0xdfe4;

    public static int MechanicsWordCount => 7;
    public static int PresentationWordCount => 2;

    /// <summary>Each facing displays a one-tick pose then sleeps; being shot spawns drops and deletes.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 4)
        {
            bool sleep = (index & 1) != 0;
            return new((ushort)(Left + 6 * (index / 2) + (sleep ? 4 : 0)),
                sleep ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep : (ushort)1);
        }
        ushort command = index switch
        {
            4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_MagdolliteFlame_SpawnDrops,
            5 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            _ => CommonEnemyProjectileInstructionProgramDefinitions.Delete,
        };
        return new((ushort)(Shot + 2 * (index - 4)), command);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Left + 6 * index + 2);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Magdollite-lava instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }
}
