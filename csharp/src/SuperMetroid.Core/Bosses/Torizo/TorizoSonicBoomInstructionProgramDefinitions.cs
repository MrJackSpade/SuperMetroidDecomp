using static SuperMetroid.Core.Programs.InstructionItem;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Compiled control for Bomb and Golden Torizo's sonic-boom programs at
/// $86:ADBF-$AE15. Their eleven interleaved spritemap operands use extracted artwork;
/// two packed sound IDs remain cartridge audio data.
/// </summary>
internal abstract class TorizoSonicBoomInstructionProgramDefinitions
{
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

    /// <summary>Native program bank $86.</summary>
    internal const byte Bank = 0x86;

    /// <summary>Compiles launch, travel, and wall-impact control while leaving eleven frame operands to extracted presentation.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xadbf),
        Entry(FiredLeft),
        Op((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6),
        Skip(1),
        Frame(6),
        Frame(6),
        Entry(MovingLeft),
        Frame(80),
        Op((ushort)EnemyProjectileInstruction.GotoY, MovingLeft),
        Entry(FiredRight),
        Op((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6),
        Skip(1),
        Frame(6),
        Frame(6),
        Entry(MovingRight),
        Frame(80),
        Op((ushort)EnemyProjectileInstruction.GotoY, MovingRight),
        Entry(WallImpact),
        Op((ushort)EnemyProjectileInstruction.ClearPreInstruction),
        Op((ushort)EnemyProjectileInstruction.DisableCollisionWIthSamusProj),
        Op((ushort)EnemyProjectileInstruction.DisableCollisionWithSamus),
        Op((ushort)EnemyProjectileInstruction.SetHighPriority),
        Op((ushort)EnemyProjectileInstruction.TimerInY, 0x0005),
        Entry(WallImpactLoop),
        Op((ushort)EnemyProjectileInstruction.Torizo_ResetPosition),
        Op((ushort)EnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius, InstructionWord.Bytes(0x0f, 0x00), InstructionWord.Bytes(0x1f, 0x00)),
        Frame(2),
        Frame(2),
        Frame(3),
        Frame(3),
        Frame(2),
        Op((ushort)EnemyProjectileInstruction.RTS_8681DE),
        Op((ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero, WallImpactLoop),
        Op((ushort)EnemyProjectileInstruction.Delete));
    /// <summary>Number of artwork operands interleaved with the compiled sonic-boom commands.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    /// <summary>Locates an artwork operand in the bank-$86 sonic-boom program.</summary>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Selects the Bomb or Golden Torizo projectile families that execute these shared programs.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoSonicBoom or
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom;

    /// <summary>Reads a compiled control word and rejects addresses belonging to artwork or unrelated bank data.</summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo sonic-boom mechanics pointer $86:{address:X4} is not compiled.");
}
