using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

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

    /// <summary>Describes the compiled instruction and presentation-word slots for the Bomb and Golden Torizo sonic-boom programs.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xadbf),
        Entry(FiredLeft),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        Skip(1),
        Frame(6),
        Frame(6),
        Entry(MovingLeft),
        Frame(80),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, MovingLeft),
        Entry(FiredRight),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        Skip(1),
        Frame(6),
        Frame(6),
        Entry(MovingRight),
        Frame(80),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, MovingRight),
        Entry(WallImpact),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_DisableCollisionWIthSamusProj),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_DisableCollisionWithSamus),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_SetHighPriority),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY, 0x0005),
        Entry(WallImpactLoop),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition),
        Op(EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius, InstructionWord.Bytes(0x0f, 0x00), InstructionWord.Bytes(0x1f, 0x00)),
        Frame(2),
        Frame(2),
        Frame(3),
        Frame(3),
        Frame(2),
        Op(EnemyProjectileCodePointers.RTS_8681DE),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero, WallImpactLoop),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete));

    /// <summary>Gets the number of interleaved spritemap operands extracted from the sonic-boom programs.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Gets the native address of an extracted spritemap operand.</summary>
    /// <param name="index">Zero-based position among the presentation slots in the compiled layout.</param>
    /// <returns>The address of the operand in bank $86.</returns>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Determines whether a projectile kind is either Bomb Torizo's or Golden Torizo's sonic boom.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns><see langword="true"/> for either sonic-boom projectile; otherwise, <see langword="false"/>.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoSonicBoom or
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom;

    /// <summary>Resolves a bank-$86 address to its compiled sonic-boom instruction word.</summary>
    /// <param name="address">Address of the mechanics word within the native program bank.</param>
    /// <returns>The compiled instruction value at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify compiled sonic-boom mechanics.</exception>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo sonic-boom mechanics pointer $86:{address:X4} is not compiled.");
}
