using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb/Golden Torizo's right- and left-foot landing-dust programs
/// at $86:AF9D-$AFCB. Their eight spritemap operands use extracted presentation art.
/// </summary>
internal abstract class TorizoLandingDustInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_TorizoLandingDustCloud_RightFoot</c> at $86:AF9D.</summary>
    internal const ushort RightFoot = 0xaf9d;
    /// <summary><c>InstList_EnemyProjectile_TorizoLandingDustCloud_LeftFoot</c> at $86:AFB5.</summary>
    internal const ushort LeftFoot = 0xafb5;

    /// <summary>Native program bank $86.</summary>
    internal const byte Bank = 0x86;

    /// <summary>Compiles both foot-specific dust sequences with fixed timing and extracted sprite selectors.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xaf9d),
        Entry(RightFoot),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        Entry(LeftFoot),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete));
    /// <summary>The number of interleaved sprite-selector words in the two dust sequences.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Gets the bank address of one landing-dust sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot across both foot programs.</param>
    /// <returns>The address of the selected presentation word in bank $86.</returns>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Reports whether a projectile kind is one of the two Torizo landing-dust clouds.</summary>
    /// <param name="kind">The projectile kind to classify.</param>
    /// <returns><see langword="true"/> for either foot's dust cloud; otherwise, <see langword="false"/>.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoRightFootDust or
        RoomEnemyProjectileKind.BombTorizoLeftFootDust;

    /// <summary>Reads a fixed timing or control operand from the compiled landing-dust programs.</summary>
    /// <param name="address">The bank-$86 address of the mechanics word.</param>
    /// <returns>The compiled word at that address.</returns>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo landing-dust mechanics pointer $86:{address:X4} is not compiled.");
}
