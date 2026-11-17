using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="KagoBugProjectileCue"/> naming what it does.
/// </summary>
internal static class KagoBugProjectileAnimations
{
    /// <summary>
    /// How long a landed bug idles, 32767 updates (just over nine minutes), before its animation starts the landing again
    /// (#1607), kept exact (#1627): when it runs out the landing replays and raises its idle cue again, which gameplay sees.
    /// The falling and jumping poses are held instead: their animations show the same pose again, so they rest on it for good.
    /// </summary>
    private const int LandedIdleDuration = 32767;

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoBugProjectileSegment, EnemyProjectileFrameId, KagoBugProjectileCue> Segments { get; } = new(
        new Dictionary<KagoBugProjectileSegment, AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>[]>
    {
        [P.Landed] =
        [
            Show(EnemyProjectileFrameId.KagoBugCrouched, 5),
            Cue(new(K.StartIdle)),
            Show(EnemyProjectileFrameId.KagoBugStanding, LandedIdleDuration),
        ],
        [P.Falling] =
        [
            Hold(EnemyProjectileFrameId.KagoBugStanding),
        ],
        [P.JumpStart] =
        [
            Show(EnemyProjectileFrameId.KagoBugCrouched, 16),
            Show(EnemyProjectileFrameId.KagoBugStanding, 5),
            Cue(new(K.StartJump)),
            Hold(EnemyProjectileFrameId.KagoBugLeaping),
        ],
        [P.Shot] =
        [
            Show(EnemyProjectileFrameId.KagoBugSpark, 4),
            Show(EnemyProjectileFrameId.KagoBugFlareBall, 4),
            Show(EnemyProjectileFrameId.KagoBugFlareRingCored, 4),
            Show(EnemyProjectileFrameId.KagoBugFlareRing, 4),
            Show(EnemyProjectileFrameId.KagoBugFlareRingFading, 4),
            Cue(new(K.SpawnDrop)),
            Cue(new(K.Delete)),
        ],
        [P.Waiting] =
        [
            Hold(EnemyProjectileFrameId.KagoBugCurledUp),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KagoBugProjectileSegment, Animation<KagoBugProjectileSegment>> StartingAt { get; } = new Dictionary<KagoBugProjectileSegment, Animation<KagoBugProjectileSegment>>
    {
        [P.Landed] = new([Whole(P.Landed)], AnimationEnd.LoopsFrom(0)),
        [P.Falling] = new([Whole(P.Falling)], AnimationEnd.Holds),
        [P.JumpStart] = new([Whole(P.JumpStart)], AnimationEnd.Holds),
        [P.Shot] = new([Whole(P.Shot)], AnimationEnd.Holds),
        [P.Waiting] = new([Whole(P.Waiting)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KagoBugProjectileSegment> StartingWith(KagoBugProjectileSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<KagoBugProjectileSegment> Whole(KagoBugProjectileSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue> Cue(KagoBugProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.Event(cue);
}
