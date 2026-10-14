using static SuperMetroid.Core.Programs.InstructionItem;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

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

    /// <summary>Right- and left-foot four-frame dust lists, each spawning clouds between waits and deleting at completion.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xaf9d),
        Entry(RightFoot),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.TorizoLandingDustClouds),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.TorizoLandingDustClouds),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.TorizoLandingDustClouds),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.Delete),
        Entry(LeftFoot),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.TorizoLandingDustClouds),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.TorizoLandingDustClouds),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.TorizoLandingDustClouds),
        Frame(4),
        Op((ushort)EnemyProjectileInstruction.Delete));
    /// <summary>Eight OAM operands across the two landing-dust lists.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    /// <summary>Finds a selected bank-$86 dust-cloud spritemap operand without exposing its spawn callback.</summary>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Admits the two Bomb Torizo foot-dust projectile kinds handled by these native lists.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoRightFootDust or
        RoomEnemyProjectileKind.BombTorizoLeftFootDust;

    /// <summary>Reads a compiled dust timing, cloud-spawn, or delete word and rejects unknown addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo landing-dust mechanics pointer $86:{address:X4} is not compiled.");
}
