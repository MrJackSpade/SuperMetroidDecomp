using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterAcidSpitSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="KiHunterAcidSpitCue"/>.
/// </summary>
internal static class KiHunterAcidSpitAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterAcidSpitSegment, EnemyProjectileFrameId, KiHunterAcidSpitCue> Segments { get; } = new(
        new Dictionary<KiHunterAcidSpitSegment, AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftDroplet, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftStreak, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftBlob, 4),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobTrailing, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobSpattering, 1),
            Cue(KiHunterAcidSpitCue.LaunchLeft),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobSpatteringTwin, 1),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftBall, 1),
        ],
        [P.HitFloor] =
        [
            Cue(KiHunterAcidSpitCue.StopMoving),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashMound, 12),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashSpreading, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashWide, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashFlat, 8),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashFading, 8),
            Cue(KiHunterAcidSpitCue.Delete),
        ],
        [P.Right] =
        [
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightDroplet, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightStreak, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightBlob, 4),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightBlobTrailing, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightBlobSpattering, 1),
            Cue(KiHunterAcidSpitCue.LaunchRight),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightBlobSpatteringTwin, 1),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightBall, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>> StartingAt { get; } = new Dictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>>
    {
        [P.Left] = new([new(P.Left, 0, 8, 1)], AnimationEnd.Holds),
        [P.HitFloor] = new([new(P.HitFloor, 0, 7, 1)], AnimationEnd.Holds),
        [P.Right] = new([new(P.Right, 0, 8, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterAcidSpitSegment> StartingWith(KiHunterAcidSpitSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue> Cue(KiHunterAcidSpitCue cue) => new AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>.Event(cue);
}
