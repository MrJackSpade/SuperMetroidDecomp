using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheFireballSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Fune and Namihe's fireball animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class FuneNamiheFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<FuneNamiheFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FuneNamiheFireballSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.Left] =
        [
            Show(EnemyProjectileFrameId.FuneNamiheFireballLeftTrailing, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballLeftRound, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballLeftDissolving, 5),
        ],
        [P.Right] =
        [
            Show(EnemyProjectileFrameId.FuneNamiheFireballRightTrailing, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballRightRound, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballRightDissolving, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FuneNamiheFireballSegment, Animation<FuneNamiheFireballSegment>> StartingAt { get; } = new Dictionary<FuneNamiheFireballSegment, Animation<FuneNamiheFireballSegment>>
    {
        [P.Left] = Animation<FuneNamiheFireballSegment>.Looping([Segments.Whole(P.Left)]),
        [P.Right] = Animation<FuneNamiheFireballSegment>.Looping([Segments.Whole(P.Right)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FuneNamiheFireballSegment> StartingWith(FuneNamiheFireballSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
