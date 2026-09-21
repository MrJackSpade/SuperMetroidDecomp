namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoLandingDustInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb/Golden Torizo's right- and left-foot landing-dust programs
/// at $86:AF9D-$AFCB. Their eight spritemap operands remain live presentation data.
/// </summary>
internal static class TorizoLandingDustInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_TorizoLandingDustCloud_RightFoot</c> at $86:AF9D.</summary>
    internal const ushort RightFoot = 0xaf9d;
    /// <summary><c>InstList_EnemyProjectile_TorizoLandingDustCloud_LeftFoot</c> at $86:AFB5.</summary>
    internal const ushort LeftFoot = 0xafb5;

    private static readonly TorizoLandingDustInstructionMechanicsWord[] Words =
    [
        new(RightFoot, 4),
        new(0xafa1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        new(0xafa3, 4),
        new(0xafa7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        new(0xafa9, 4),
        new(0xafad, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        new(0xafaf, 4),
        new(0xafb3, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(LeftFoot, 4),
        new(0xafb9, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        new(0xafbb, 4),
        new(0xafbf, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        new(0xafc1, 4),
        new(0xafc5, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        new(0xafc7, 4),
        new(0xafcb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xaf9f, 0xafa5, 0xafab, 0xafb1,
        0xafb7, 0xafbd, 0xafc3, 0xafc9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static TorizoLandingDustInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoRightFootDust or
        RoomEnemyProjectileKind.BombTorizoLeftFootDust;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoLandingDustInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo landing-dust mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (TorizoLandingDustInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
