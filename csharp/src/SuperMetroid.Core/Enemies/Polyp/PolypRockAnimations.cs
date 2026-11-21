using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Polyp.PolypRockSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// The Polyp's lava-rock animation (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class PolypRockAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PolypRockSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<PolypRockSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.LavaRock] =
        [
            Hold(EnemyProjectileFrameId.PolypRock),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PolypRockSegment, Animation<PolypRockSegment>> StartingAt { get; } = new Dictionary<PolypRockSegment, Animation<PolypRockSegment>>
    {
        [P.LavaRock] = new([Segments.Whole(P.LavaRock)], AnimationEnd.Holds),
    };

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.HeldFrame(frame);
}
