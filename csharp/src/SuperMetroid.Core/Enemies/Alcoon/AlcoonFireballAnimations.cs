using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonFireballSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Alcoon's fireball animation (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class AlcoonFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<AlcoonFireballSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Spinning] =
        [
            Show(EnemyProjectileFrameId.AlcoonFireballTurned0, 3),
            Show(EnemyProjectileFrameId.AlcoonFireballTurned90, 3),
            Show(EnemyProjectileFrameId.AlcoonFireballTurned180, 3),
            Show(EnemyProjectileFrameId.AlcoonFireballTurned270, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AlcoonFireballSegment, Animation<AlcoonFireballSegment>> StartingAt { get; } = new Dictionary<AlcoonFireballSegment, Animation<AlcoonFireballSegment>>
    {
        [P.Spinning] = Animation<AlcoonFireballSegment>.Looping([Segments.Whole(P.Spinning)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AlcoonFireballSegment> StartingWith(AlcoonFireballSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
