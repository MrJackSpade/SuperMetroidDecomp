namespace SuperMetroid.Core.Game;

/// <summary>One compiled Phantoon projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct PhantoonProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Phantoon's starting and destroyable flame instruction lists.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class PhantoonProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PhantoonDestroyableFlame_Idle</c> at $86:975C.</summary>
    internal const ushort DestroyableIdle = 0x975c;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround</c> at $86:976C.</summary>
    internal const ushort CasualHitGround = 0x976c;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing</c> at $86:9772.</summary>
    internal const ushort CasualBouncing = 0x9772;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Resetting</c> at $86:9782.</summary>
    internal const ushort CasualResting = 0x9782;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Dying</c> at $86:979A.</summary>
    internal const ushort Dying = 0x979a;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Rain_Falling</c> at $86:97AC.</summary>
    internal const ushort RainImpact = 0x97ac;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Falling_0</c> at $86:97B4.</summary>
    internal const ushort CasualFalling = 0x97b4;

    /// <summary><c>InstList_EnemyProjectile_PhantoonStartingFlames</c> at $86:97E8.</summary>
    internal const ushort StartingFlame = 0x97e8;

    /// <summary><c>InstList_EnemyProjectile_PhantoonDestroyableFlame_Delete</c> at $86:97F8.</summary>
    internal const ushort Delete = 0x97f8;

    /// <summary><c>InstList_EnemyProjectile_Shot_PhantoonDestroyableFlames</c> at $86:97FA.</summary>
    internal const ushort DestroyableShot = 0x97fa;

    private static readonly PhantoonProjectileInstructionMechanicsWord[] Words =
    [
        new(DestroyableIdle, 0x0005),
        new(0x9760, 0x0005),
        new(0x9764, 0x0005),
        new(0x9768, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x976a, DestroyableIdle),
        new(CasualHitGround, 0x0001),
        new(0x9770, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(CasualBouncing, 0x0005),
        new(0x9776, 0x0005),
        new(0x977a, 0x0005),
        new(0x977e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x9780, CasualBouncing),
        new(CasualResting, 0x0005),
        new(0x9786, 0x0005),
        new(0x978a, 0x0005),
        new(0x978e, 0x0005),
        new(0x9792, 0x0005),
        new(0x9796, 0x0005),
        new(Dying, 0x0005),
        new(0x979e, 0x0005),
        new(0x97a2, 0x0005),
        new(0x97a6, 0x0005),
        new(0x97aa, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(RainImpact, 0x0008),
        new(0x97b0, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x97b2, Dying),
        new(CasualFalling, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0x97b6, 0x0004),
        new(0x97b8, 0x0001),
        new(0x97bc, 0x0001),
        new(0x97c0, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0x97c2, 0x97b8),
        new(0x97c4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0x97c6, 0x0004),
        new(0x97c8, 0x0001),
        new(0x97cc, 0x0001),
        new(0x97d0, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0x97d2, 0x97c8),
        new(0x97d4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0x97d6, 0x0004),
        new(0x97d8, 0x0001),
        new(0x97dc, 0x0001),
        new(0x97e0, EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0x97e2, 0x97d8),
        new(0x97e4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x97e6, CasualFalling),
        new(StartingFlame, 0x0005),
        new(0x97ec, 0x0005),
        new(0x97f0, 0x0005),
        new(0x97f4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x97f6, StartingFlame),
        new(Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(DestroyableShot, 0x0005),
        new(0x97fe, 0x0005),
        new(0x9802, 0x0005),
        new(0x9806, 0x0005),
        new(0x980a, EnemyProjectileCodePointers.Instruction_SpawnPhantoonDrop),
        new(0x980c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x975e, 0x9762, 0x9766, 0x976e,
        0x9774, 0x9778, 0x977c,
        0x9784, 0x9788, 0x978c, 0x9790, 0x9794, 0x9798,
        0x979c, 0x97a0, 0x97a4, 0x97a8,
        0x97ae,
        0x97ba, 0x97be, 0x97ca, 0x97ce, 0x97da, 0x97de,
        0x97ea, 0x97ee, 0x97f2,
        0x97fc, 0x9800, 0x9804, 0x9808,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static PhantoonProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.PhantoonDestroyableFlame or
        RoomEnemyProjectileKind.PhantoonStartingFlame;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PhantoonProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Phantoon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
