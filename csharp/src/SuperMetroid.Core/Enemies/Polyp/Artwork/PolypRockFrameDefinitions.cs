using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp.Artwork;

/// <summary>
/// The presentation boundary of the Polyp's lava rock programs (#641): the projectile frame each
/// <see cref="PolypRockFrame"/> pose shows.
/// </summary>
internal static class PolypRockFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 1;

    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(PolypRockFrame frame) => frame.Pose switch
    {
        0 => 0x9340,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Polyp lava rock pose."),
    };

    /// <summary>The pose whose installed artwork key is <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out PolypRockFrame frame)
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
    internal static IReadOnlyDictionary<PolypRockFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<PolypRockFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.PolypRock00,
        };
}
