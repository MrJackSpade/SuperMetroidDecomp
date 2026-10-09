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

    /// <summary>Number of address/value entries compiling the Eye Door projectile instruction lists.</summary>
    public static int MechanicsWordCount => 19;

    /// <summary>Number of spritemap operands referenced by the compiled projectile lists.</summary>
    public static int PresentationWordCount => 11;

    /// <summary>Gets the native address and compiled value of a mechanics word by flattened program order.</summary>
    /// <param name="index">Zero-based index across the initial, flight, impact, and shot instruction lists.</param>
    /// <returns>The ROM address and value represented at that position.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return new((ushort)(Initial + 4 * index), (ushort)(4 - index));
        if (index < 6)
            return new((ushort)(Initial + 12 + 2 * (index - 3)), index switch
            {
                3 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_CalculateDirectionTowardsSamus,
                4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY,
                _ => EyeDoorEnemyProjectileRomData.ProjectilePreInstruction,
            });
        if (index < 9)
            return new((ushort)(FlyingLoop + (index == 6 ? 0 : 2 + 2 * (index - 6))), index switch
            {
                6 => 16,
                7 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
                _ => FlyingLoop,
            });
        if (index == 9) return new(Impact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction);
        if (index < 13) return new((ushort)(Impact + 2 + 4 * (index - 10)), (ushort)(index - 8));
        if (index == 13) return new((ushort)(Shot - 2), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        return new((ushort)(Shot + 4 * (index - 14)), index < 18 ? (ushort)4 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
    }

    /// <summary>Gets the native ROM address of a referenced spritemap operand by program order.</summary>
    /// <param name="index">Zero-based index among the compiled presentation operands.</param>
    /// <returns>The address containing the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation words.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return (ushort)(Initial + 2 + 4 * index);
        if (index == 3) return FlyingLoop + 2;
        if (index < 7) return (ushort)(Impact + 4 + 4 * (index - 4));
        return (ushort)(Shot + 2 + 4 * (index - 7));
    }

    /// <summary>Resolves a native mechanics-word address to its compiled instruction value.</summary>
    /// <param name="address">ROM address of a word in one of the compiled Eye Door lists.</param>
    /// <returns>The compiled value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of a compiled instruction list.</exception>
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
