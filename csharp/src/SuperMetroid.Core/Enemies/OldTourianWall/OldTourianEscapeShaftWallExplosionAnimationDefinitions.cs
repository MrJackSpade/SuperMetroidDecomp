using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>
/// The old Tourian escape shaft fake-wall explosion, whose animation
/// <see cref="OldTourianWallExplosionAnimation"/> plays typed. Its six small-explosion frames resolve
/// through extracted presentation art.
/// </summary>
internal static class OldTourianEscapeShaftWallExplosionAnimationDefinitions
{
    /// <summary>The wall-middle X the explosions are placed at.</summary>
    internal const int XPosition = 272;
    /// <summary>The wall-middle Y the explosions are placed at.</summary>
    internal const int YPosition = 2184;

    /// <summary>How many times the burst plays: once.</summary>
    internal const int BurstCount = 1;

    /// <summary>
    /// The random scatter's fields, zero to seven pixels across (three bits) and zero to fifteen down (four bits), each
    /// centred on zero.
    /// </summary>
    internal static EnemyProjectileScatter Scatter => new(XRange: new(0, 3), YRange: new(0, 4), Center: new(0, 0));

    /// <summary>The sound each burst's explosion makes.</summary>
    internal static SoundEffectId ExplosionSound => EnemySound.SmallExplosion;
    /// <summary>The priority of each burst's explosion sound.</summary>
    internal static readonly SoundRequestPriority ExplosionSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The six small-explosion frames the burst shows, in order (#1817), each with how long it holds (#2044).</summary>
    internal static IReadOnlyList<OldTourianWallExplosionBeat> Beats { get; } =
    [
        new(EnemyProjectileFrameId.OldTourianEscapeWallExplosionFirst, 4),
        new(EnemyProjectileFrameId.OldTourianEscapeWallExplosionSecond, 6),
        new(EnemyProjectileFrameId.OldTourianEscapeWallExplosionThird, 5),
        new(EnemyProjectileFrameId.OldTourianEscapeWallExplosionFourth, 5),
        new(EnemyProjectileFrameId.OldTourianEscapeWallExplosionFifth, 5),
        new(EnemyProjectileFrameId.OldTourianEscapeWallExplosionSixth, 6),
    ];
}
