namespace SuperMetroid.Core.Game;

internal readonly record struct TourianStatueProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the Tourian entrance-statue actors at $86:B79F-$B878. The
/// twenty-eight interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class TourianStatueProjectileInstructionProgramDefinitions
{
    /// <summary>Private Tourian projectile deletion program at $86:B79F.</summary>
    internal const ushort Delete = 0xb79f;
    /// <summary>Unlocking-particle water splash program at $86:B7A1.</summary>
    internal const ushort Splash = 0xb7a1;
    /// <summary>Boss-statue eye-glow program at $86:B7B3.</summary>
    internal const ushort EyeGlow = 0xb7b3;
    /// <summary>Unlocking-particle program at $86:B802.</summary>
    internal const ushort Particle = 0xb802;
    /// <summary>Unlocking-particle tail program at $86:B823.</summary>
    internal const ushort Tail = 0xb823;
    /// <summary>Ascending statue-soul program at $86:B84E.</summary>
    internal const ushort Soul = 0xb84e;
    /// <summary>Initial base-decoration program at $86:B85A.</summary>
    internal const ushort BaseDecoration = 0xb85a;
    /// <summary>Looping base-decoration program at $86:B862.</summary>
    internal const ushort BaseDecorationLoop = 0xb862;
    /// <summary>Looping Ridley-statue program at $86:B86A.</summary>
    internal const ushort Ridley = 0xb86a;
    /// <summary>Looping Phantoon-statue program at $86:B872.</summary>
    internal const ushort Phantoon = 0xb872;

    private static readonly TourianStatueProjectileInstructionMechanicsWord[] Words =
    [
        new(Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(Splash, 8), new(0xb7a5, 8), new(0xb7a9, 8), new(0xb7ad, 8),
        new(0xb7b1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(EyeGlow, 8), new(0xb7b7, 8), new(0xb7bb, 8),
        new(0xb7bf, 7), new(0xb7c3, 7), new(0xb7c7, 7),
        new(0xb7cb, 6), new(0xb7cf, 6), new(0xb7d3, 5), new(0xb7d7, 0x30),
        new(0xb7db, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xb7de, TourianStatueRomData.Earthquake),
        new(0xb7e0, TourianStatueRomData.SpawnParticle),
        new(0xb7e2, TourianStatueRomData.SpawnParticle),
        new(0xb7e4, TourianStatueRomData.SpawnParticle),
        new(0xb7e6, TourianStatueRomData.SpawnParticle),
        new(0xb7e8, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(Particle, 3), new(0xb806, 3), new(0xb80a, TourianStatueRomData.SpawnTail),
        new(0xb80c, 3), new(0xb810, 3),
        new(0xb814, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xb816, Particle),
        new(Tail, 4), new(0xb827, TourianStatueRomData.AddY), new(0xb829, 8),
        new(0xb82b, 4), new(0xb82f, TourianStatueRomData.AddY), new(0xb831, 4),
        new(0xb833, 4), new(0xb837, TourianStatueRomData.AddY), new(0xb839, 2),
        new(0xb83b, 4), new(0xb83f, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(Soul, 8), new(0xb852, 8),
        new(0xb856, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb858, Soul),
        new(BaseDecoration, 0x80),
        new(0xb85e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xb860, EnemyProjectileCodePointers.PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess),
        new(BaseDecorationLoop, 0x0777),
        new(0xb866, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb868, BaseDecorationLoop),
        new(Ridley, 0x0777),
        new(0xb86e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb870, Ridley),
        new(Phantoon, 0x0777),
        new(0xb876, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb878, Phantoon),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb7a3, 0xb7a7, 0xb7ab, 0xb7af,
        0xb7b5, 0xb7b9, 0xb7bd, 0xb7c1, 0xb7c5, 0xb7c9, 0xb7cd, 0xb7d1, 0xb7d5, 0xb7d9,
        0xb804, 0xb808, 0xb80e, 0xb812,
        0xb825, 0xb82d, 0xb835, 0xb83d,
        0xb850, 0xb854, 0xb85c, 0xb864, 0xb86c, 0xb874,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static TourianStatueProjectileInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.TourianStatueSplash or
        RoomEnemyProjectileKind.TourianStatueEyeGlow or
        RoomEnemyProjectileKind.TourianStatueParticle or
        RoomEnemyProjectileKind.TourianStatueTail or
        RoomEnemyProjectileKind.TourianStatueSoul or
        RoomEnemyProjectileKind.TourianStatueRidley or
        RoomEnemyProjectileKind.TourianStatuePhantoon or
        RoomEnemyProjectileKind.TourianStatueBaseDecoration;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TourianStatueProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Tourian statue projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (TourianStatueProjectileInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
