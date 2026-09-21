namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoSonicBoomInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb and Golden Torizo's sonic-boom programs at
/// $86:ADBF-$AE15. Their eleven interleaved spritemap operands and two packed sound IDs
/// remain live cartridge presentation/audio data.
/// </summary>
internal static class TorizoSonicBoomInstructionProgramDefinitions
{
    /// <summary><c>InitAI_EnemyProjectile_TorizoSonicBoom</c> at $86:AE15.</summary>
    internal const ushort InitializationAi = 0xae15;
    /// <summary><c>InstList_EnemyProjectile_TorizoSonicBoom_FiredLeft</c> at $86:ADBF.</summary>
    internal const ushort FiredLeft = 0xadbf;
    /// <summary><c>InstList_EnemyProjectile_TorizoSonicBoom_MovingLeft</c> at $86:ADCA.</summary>
    internal const ushort MovingLeft = 0xadca;
    /// <summary><c>InstList_EnemyProjectile_TorizoSonicBoom_FiredRight</c> at $86:ADD2.</summary>
    internal const ushort FiredRight = 0xadd2;
    /// <summary><c>InstList_EnemyProjectile_TorizoSonicBoom_MovingRight</c> at $86:ADDD.</summary>
    internal const ushort MovingRight = 0xaddd;
    /// <summary><c>InstList_EnemyProjectile_TorizoSonicBoom_HitWall_0</c> at $86:ADE5.</summary>
    internal const ushort WallImpact = 0xade5;
    /// <summary><c>InstList_EnemyProjectile_TorizoSonicBoom_HitWall_1</c> at $86:ADF1.</summary>
    internal const ushort WallImpactLoop = 0xadf1;

    private static readonly TorizoSonicBoomInstructionMechanicsWord[] Words =
    [
        new(FiredLeft, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xadc2, 6),
        new(0xadc6, 6),
        new(MovingLeft, 0x0050),
        new(0xadce, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xadd0, MovingLeft),

        new(FiredRight, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xadd5, 6),
        new(0xadd9, 6),
        new(MovingRight, 0x0050),
        new(0xade1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xade3, MovingRight),

        new(WallImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xade7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DisableCollisionWIthSamusProj),
        new(0xade9, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DisableCollisionWithSamus),
        new(0xadeb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_SetHighPriority),
        new(0xaded, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xadef, 5),
        new(WallImpactLoop, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition),
        new(0xadf3, EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xadf5, 0x000f),
        new(0xadf7, 0x001f),
        new(0xadf9, 2),
        new(0xadfd, 2),
        new(0xae01, 3),
        new(0xae05, 3),
        new(0xae09, 2),
        new(0xae0d, EnemyProjectileCodePointers.RTS_8681DE),
        new(0xae0f, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xae11, WallImpactLoop),
        new(0xae13, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xadc4, 0xadc8, 0xadcc,
        0xadd7, 0xaddb, 0xaddf,
        0xadfb, 0xadff, 0xae03, 0xae07, 0xae0b,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static TorizoSonicBoomInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoSonicBoom or
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoSonicBoomInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo sonic-boom mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (TorizoSonicBoomInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
