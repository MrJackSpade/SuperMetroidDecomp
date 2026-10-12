namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the Eye Door's aimed projectile, wall-impact animation, and shot
/// animation. Interleaved spritemap operands identify compiled presentation.
/// </summary>
internal abstract class EyeDoorProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Normal_0</c> at $86:B5D9.</summary>
    internal const ushort Initial = 0xb5d9;

    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Normal_1</c>, flight loop at $86:B5EB.</summary>
    internal const ushort FlyingLoop = 0xb5eb;

    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Explode</c> at $86:B5F3.</summary>
    internal const ushort Impact = 0xb5f3;

    /// <summary><c>InstList_EnemyProjectile_Shot_EyeDoorProjectile</c> at $86:B603.</summary>
    internal const ushort Shot = 0xb603;

    public static int MechanicsWordCount => 19;
    public static int PresentationWordCount => 11;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return new((ushort)(Initial + 4 * index), (ushort)(4 - index));
        if (index < 6)
            return new((ushort)(Initial + 12 + 2 * (index - 3)), index switch
            {
                3 => (ushort)EnemyProjectileInstruction.CalculateDirectionTowardsSamus,
                4 => (ushort)EnemyProjectileInstruction.PreInstructionInY,
                _ => (ushort)EnemyProjectilePreInstruction.EyeDoorProjectilePreInstruction,
            });
        if (index < 9)
            return new((ushort)(FlyingLoop + (index == 6 ? 0 : 2 + 2 * (index - 6))), index switch
            {
                6 => 16,
                7 => (ushort)EnemyProjectileInstruction.GotoY,
                _ => FlyingLoop,
            });
        if (index == 9) return new(Impact, (ushort)EnemyProjectileInstruction.ClearPreInstruction);
        if (index < 13) return new((ushort)(Impact + 2 + 4 * (index - 10)), (ushort)(index - 8));
        if (index == 13) return new((ushort)(Shot - 2), (ushort)EnemyProjectileInstruction.Delete);
        return new((ushort)(Shot + 4 * (index - 14)), index < 18 ? (ushort)4 : (ushort)EnemyProjectileInstruction.Delete);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return (ushort)(Initial + 2 + 4 * index);
        if (index == 3) return FlyingLoop + 2;
        if (index < 7) return (ushort)(Impact + 4 + 4 * (index - 4));
        return (ushort)(Shot + 2 + 4 * (index - 7));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException($"Eye Door projectile instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}