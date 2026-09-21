namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoExplosiveSwipeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb Torizo's explosive-swipe projectile at $86:A4AA-$A4C1.
/// Its five spritemap operands and packed sound ID remain live presentation/audio data.
/// </summary>
internal static class TorizoExplosiveSwipeInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BombTorizoExplosionSwipe</c> at $86:A4AA.</summary>
    internal const ushort Initial = 0xa4aa;

    private static readonly TorizoExplosiveSwipeInstructionMechanicsWord[] Words =
    [
        new(Initial, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1),
        new(0xa4ad, 5),
        new(0xa4b1, 5),
        new(0xa4b5, 5),
        new(0xa4b9, 5),
        new(0xa4bd, 5),
        new(0xa4c1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xa4af, 0xa4b3, 0xa4b7, 0xa4bb, 0xa4bf,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static TorizoExplosiveSwipeInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoExplosiveSwipeInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo explosive-swipe mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (TorizoExplosiveSwipeInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
