using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>
/// Enemy projectile $86:B4B1, the old Tourian escape shaft fake-wall explosion, and its list
/// <c>InstList_EnemyProj_OldTourianEscapeShaftFakeWallExplosion</c> at $86:B443-$B46F, which
/// <see cref="OldTourianWallExplosionAnimation"/> plays typed. Its six small-explosion spritemap operands resolve
/// through extracted presentation art.
/// </summary>
internal static class OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions
{
    /// <summary>$86:B4B0, the header's bare-RTS pre-instruction.</summary>
    internal const EnemyProjectilePreInstruction PreInstruction = EnemyProjectilePreInstruction.RTS_86B4B0;

    /// <summary>$86:B4A0/$B4A6: the wall-middle X/Y written to position and Var0/Var1.</summary>
    internal const ushort XPosition = 0x0110;
    /// <summary>$86:B4A6 wall-middle Y used by both the projectile position and Var1.</summary>
    internal const ushort YPosition = 0x0888;

    /// <summary>$86:B447: the loop count the list loads into the general timer; the burst plays once.</summary>
    internal const int BurstCount = 1;

    /// <summary>$86:B44B: the random scatter's masks, seven pixels across and fifteen down, each centred on zero.</summary>
    internal static EnemyProjectileScatter Scatter => new(new(7, 15), new(0, 0));

    /// <summary>$86:B44F-$B451: library-two small explosion $24, Max6.</summary>
    internal static SoundEffectId ExplosionSound => Library2Sound.SmallExplosion;
    /// <summary>Native maximum of six queued library-two small-explosion sounds.</summary>
    internal static readonly SoundRequestPriority ExplosionSoundMaximum = SoundRequestPriority.Normal;

    /// <summary>$86:B452-$B468: six small-explosion poses holding 4, 6, 5, 5, 5 and 6 frames.</summary>
    internal static ReadOnlySpan<int> FrameDurations => [4, 6, 5, 5, 5, 6];
}
