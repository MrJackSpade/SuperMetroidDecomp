using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

/// <summary>
/// The presentation boundary of Fune and Namihe's fireball programs (#641): the projectile frame each
/// <see cref="FuneNamiheFireballFrame"/> pose shows.
/// </summary>
internal static class FuneNamiheFireballFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 6;

    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(FuneNamiheFireballFrame frame) => frame.Pose switch
    {
        0 => 0xaab9,
        1 => 0xaac0,
        2 => 0xaac7,
        3 => 0xaace,
        4 => 0xaad5,
        5 => 0xaadc,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fune/Namihe fireball pose."),
    };

    /// <summary>The pose whose installed artwork key is <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out FuneNamiheFireballFrame frame)
    {
        for (int pose = 0; pose < PoseCount; pose++)
        {
            frame = new(pose);
            if (EnemyProjectileFrames.ProgramFrameKey(ShownProjectileFrames[frame]) == key)
                return true;
        }
        frame = default;
        return false;
    }

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FuneNamiheFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FuneNamiheFireballFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.FuneNamiheFireballFrame00,
            [new(1)] = EnemyProjectileFrameId.FuneNamiheFireballFrame01,
            [new(2)] = EnemyProjectileFrameId.FuneNamiheFireballFrame02,
            [new(3)] = EnemyProjectileFrameId.FuneNamiheFireballFrame03,
            [new(4)] = EnemyProjectileFrameId.FuneNamiheFireballFrame04,
            [new(5)] = EnemyProjectileFrameId.FuneNamiheFireballFrame05,
        };
}
