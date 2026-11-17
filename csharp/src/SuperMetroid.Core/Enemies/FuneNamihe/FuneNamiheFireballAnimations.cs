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
    internal static AnimationSegments<FuneNamiheFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FuneNamiheFireballSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
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
        [P.Left] = new([Whole(P.Left)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([Whole(P.Right)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FuneNamiheFireballSegment> StartingWith(FuneNamiheFireballSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<FuneNamiheFireballSegment> Whole(FuneNamiheFireballSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
