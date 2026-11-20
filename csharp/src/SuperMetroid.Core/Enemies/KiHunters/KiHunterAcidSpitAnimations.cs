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
        [P.Left] = Spat(EnemyProjectileFrameId.KiHunterAcidSpitLeftDroplet, EnemyProjectileFrameId.KiHunterAcidSpitLeftStreak, EnemyProjectileFrameId.KiHunterAcidSpitLeftBlob, EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobTrailing, EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobSpattering, HorizontalDirection.Left, EnemyProjectileFrameId.KiHunterAcidSpitLeftBall),
        [P.HitFloor] =
        [
            Cue(new KiHunterAcidSpitCue.Signal(KiHunterAcidSpitCueKind.StopMoving)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashMound, 12),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashSpreading, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashWide, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashFlat, 8),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitSplashFading, 8),
            Cue(new KiHunterAcidSpitCue.Signal(KiHunterAcidSpitCueKind.Delete)),
        ],
        [P.Right] = Spat(EnemyProjectileFrameId.KiHunterAcidSpitRightDroplet, EnemyProjectileFrameId.KiHunterAcidSpitRightStreak, EnemyProjectileFrameId.KiHunterAcidSpitRightBlob, EnemyProjectileFrameId.KiHunterAcidSpitRightBlobTrailing, EnemyProjectileFrameId.KiHunterAcidSpitRightBlobSpattering, HorizontalDirection.Right, EnemyProjectileFrameId.KiHunterAcidSpitRightBall),
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A launched spit flies on its ball, held
    /// until it strikes the floor (#2439).
    /// </summary>
    internal static IReadOnlyDictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>> StartingAt { get; } = new Dictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>>
    {
        [P.Left] = new([Segments.Whole(P.Left)], AnimationEnd.Holds),
        [P.HitFloor] = new([Segments.Whole(P.HitFloor)], AnimationEnd.Holds),
        [P.Right] = new([Segments.Whole(P.Right)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterAcidSpitSegment> StartingWith(KiHunterAcidSpitSegment segment) => StartingAt[segment];

    /// <summary>Acid spat over a facing's pictures and launch, <paramref name="droplet"/>, <paramref name="streak"/>, <paramref name="blob"/>, <paramref name="blobTrailing"/>, <paramref name="blobSpattering"/>, <paramref name="facing"/>, <paramref name="ball"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>[] Spat(EnemyProjectileFrameId droplet, EnemyProjectileFrameId streak, EnemyProjectileFrameId blob, EnemyProjectileFrameId blobTrailing, EnemyProjectileFrameId blobSpattering, HorizontalDirection facing, EnemyProjectileFrameId ball) =>
    [
        Show(droplet, 3),
        Show(streak, 3),
        Show(blob, 4),
        Show(blobTrailing, 3),
        Show(blobSpattering, 1),
        Cue(new KiHunterAcidSpitCue.Launch(facing)),
        Show(blobSpattering, 1),
        Hold(ball),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue> Cue(KiHunterAcidSpitCue cue) => new AnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>.Event(cue);
}
