using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileProgram;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile programs (#641).</summary>
internal static class TourianStatueProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<TourianStatueProjectileProgram, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments { get; } = new(
        new Dictionary<TourianStatueProjectileProgram, AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>[]>
    {
        [P.Delete] =
        [
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.Splash] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame00, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame01, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame02, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame03, 8),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.EyeGlow] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame04, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame05, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame06, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame07, 7),
            Show(EnemyProjectileFrameId.TourianStatueFrame08, 7),
            Show(EnemyProjectileFrameId.TourianStatueFrame09, 7),
            Show(EnemyProjectileFrameId.TourianStatueFrame10, 6),
            Show(EnemyProjectileFrameId.TourianStatueFrame11, 6),
            Show(EnemyProjectileFrameId.TourianStatueFrame12, 5),
            Show(EnemyProjectileFrameId.TourianStatueFrame13, 48),
            Cue(new TourianStatueProjectileCue.Command(K.PlayReleaseSound)),
            Cue(new TourianStatueProjectileCue.Command(K.Earthquake)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.Particle] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame14, 3),
            Show(EnemyProjectileFrameId.TourianStatueFrame15, 3),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnTail)),
            Show(EnemyProjectileFrameId.TourianStatueFrame16, 3),
            Show(EnemyProjectileFrameId.TourianStatueFrame17, 3),
        ],
        [P.Tail] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame18, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 8)),
            Show(EnemyProjectileFrameId.TourianStatueFrame19, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 4)),
            Show(EnemyProjectileFrameId.TourianStatueFrame20, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 2)),
            Show(EnemyProjectileFrameId.TourianStatueFrame21, 4),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.Soul] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame22, 8),
            Show(EnemyProjectileFrameId.TourianStatueFrame23, 8),
        ],
        [P.BaseDecoration] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame24, 128),
            Cue(new TourianStatueProjectileCue.StartMoving(Movement: EnemyProjectileFrameBehavior.TourianStatueRevealFollow)),
        ],
        [P.BaseDecorationLoop] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame25, 1911),
        ],
        [P.Ridley] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame26, 1911),
        ],
        [P.Phantoon] =
        [
            Show(EnemyProjectileFrameId.TourianStatueFrame27, 1911),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A particle starts its counted list
    /// directly, with the counter its spawn cleared: its first play counts that zero down, wrapping it, so the list
    /// replays the longest count more times before it holds.
    /// </summary>
    internal static IReadOnlyDictionary<TourianStatueProjectileProgram, Animation<TourianStatueProjectileProgram>> StartingAt { get; } = new Dictionary<TourianStatueProjectileProgram, Animation<TourianStatueProjectileProgram>>
    {
        [P.Delete] = new([new(P.Delete, 0, 1, 1)], AnimationEnd.Holds),
        [P.Splash] = new([new(P.Splash, 0, 5, 1)], AnimationEnd.Holds),
        [P.EyeGlow] = new([new(P.EyeGlow, 0, 17, 1)], AnimationEnd.Holds),
        [P.Particle] = new([new(P.Particle, 0, 5, 1), new(P.Particle, 0, 5, WrappingCountdown.LongestStart)], AnimationEnd.Holds),
        [P.Tail] = new([new(P.Tail, 0, 8, 1)], AnimationEnd.Holds),
        [P.Soul] = new([new(P.Soul, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BaseDecoration] = new([new(P.BaseDecoration, 0, 2, 1), new(P.BaseDecorationLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BaseDecorationLoop] = new([new(P.BaseDecorationLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ridley] = new([new(P.Ridley, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Phantoon] = new([new(P.Phantoon, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TourianStatueProjectileProgram> StartingWith(TourianStatueProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue> Cue(TourianStatueProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Event(cue);
}
