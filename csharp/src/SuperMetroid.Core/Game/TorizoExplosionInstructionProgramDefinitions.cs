namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoExplosionInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb Torizo's low-health and death-explosion programs at
/// $86:A3CB-$A455. Their fifteen interleaved spritemap operands remain live cartridge
/// presentation data.
/// </summary>
internal static class TorizoExplosionInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BombTorizoLowHealthExplosion_0</c> at $86:A3CB.</summary>
    internal const ushort LowHealthInitial = 0xa3cb;
    /// <summary><c>InstList_EnemyProjectile_BombTorizoLowHealthExplosion_1</c> at $86:A3D5.</summary>
    internal const ushort LowHealthLoop = 0xa3d5;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_0</c> at $86:A3FA.</summary>
    internal const ushort DeathInitial = 0xa3fa;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_1</c> at $86:A408.</summary>
    internal const ushort DeathExplosionLoop = 0xa408;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_2</c> at $86:A431.</summary>
    internal const ushort DeathSmokeSetup = 0xa431;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_3</c> at $86:A435.</summary>
    internal const ushort DeathSmokeLoop = 0xa435;

    private static readonly TorizoExplosionInstructionMechanicsWord[] Words =
    [
        new(LowHealthInitial, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xa3cd, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xa3cf, 0x3000),
        new(0xa3d1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xa3d3, 3),
        new(LowHealthLoop, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition),
        new(0xa3d7, EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xa3d9, 0x000f),
        new(0xa3db, 0x000f),
        new(0xa3dd, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xa3e0, 2),
        new(0xa3e4, 2),
        new(0xa3e8, 3),
        new(0xa3ec, 3),
        new(0xa3f0, 2),
        new(0xa3f4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xa3f6, LowHealthLoop),
        new(0xa3f8, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(DeathInitial, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xa3fc, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xa3fe, 0x3000),
        new(0xa400, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY_Probability_1_4),
        new(0xa402, DeathSmokeSetup),
        new(0xa404, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xa406, 2),
        new(DeathExplosionLoop, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition),
        new(0xa40a, EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xa40c, 0x001f),
        new(0xa40e, 0x103f),
        new(0xa410, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xa413, 4),
        new(0xa417, 6),
        new(0xa41b, 5),
        new(0xa41f, 5),
        new(0xa423, 5),
        new(0xa427, 6),
        new(0xa42b, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xa42d, DeathExplosionLoop),
        new(0xa42f, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(DeathSmokeSetup, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xa433, 2),
        new(DeathSmokeLoop, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition),
        new(0xa437, EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xa439, 0x001f),
        new(0xa43b, 0x043f),
        new(0xa43d, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xa440, 8),
        new(0xa444, 8),
        new(0xa448, 8),
        new(0xa44c, 8),
        new(0xa450, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xa452, DeathSmokeLoop),
        new(0xa454, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xa3e2, 0xa3e6, 0xa3ea, 0xa3ee, 0xa3f2,
        0xa415, 0xa419, 0xa41d, 0xa421, 0xa425, 0xa429,
        0xa442, 0xa446, 0xa44a, 0xa44e,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static TorizoExplosionInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
        RoomEnemyProjectileKind.BombTorizoDeathExplosion;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoExplosionInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo explosion mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (TorizoExplosionInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
