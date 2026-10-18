using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>
/// The presentation boundary of Alcoon's fireball programs (#641): the projectile frame each
/// <see cref="AlcoonFireballFrame"/> pose shows.
/// </summary>
internal static class AlcoonFireballFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 4;

    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(AlcoonFireballFrame frame) => frame.Pose switch
    {
        0 => 0x8404,
        1 => 0x840b,
        2 => 0x8412,
        3 => 0x8419,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon fireball pose."),
    };

    /// <summary>The pose whose installed artwork key is <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out AlcoonFireballFrame frame)
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
    internal static IReadOnlyDictionary<AlcoonFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<AlcoonFireballFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.AlcoonFireballFrame00,
            [new(1)] = EnemyProjectileFrameId.AlcoonFireballFrame01,
            [new(2)] = EnemyProjectileFrameId.AlcoonFireballFrame02,
            [new(3)] = EnemyProjectileFrameId.AlcoonFireballFrame03,
        };
}
