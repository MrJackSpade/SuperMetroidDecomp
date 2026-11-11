using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileProgram;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile programs (#641).</summary>
internal static class TourianStatueProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Segments { get; } = new(
        new Dictionary<TourianStatueProjectileProgram, AnimationBeat<TourianStatueProjectileFrame, TourianStatueProjectileCue>[]>
    {
        [P.Delete] =
        [
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.Splash] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.EyeGlow] =
        [
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 7),
            Show(8, 7),
            Show(9, 7),
            Show(10, 6),
            Show(11, 6),
            Show(12, 5),
            Show(13, 48),
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
            Show(14, 3),
            Show(15, 3),
            Cue(new TourianStatueProjectileCue.Command(K.SpawnTail)),
            Show(16, 3),
            Show(17, 3),
        ],
        [P.Tail] =
        [
            Show(18, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 8)),
            Show(19, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 4)),
            Show(20, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 2)),
            Show(21, 4),
            Cue(new TourianStatueProjectileCue.Command(K.Delete)),
        ],
        [P.Soul] =
        [
            Show(22, 8),
            Show(23, 8),
        ],
        [P.BaseDecoration] =
        [
            Show(24, 128),
            Cue(new TourianStatueProjectileCue.StartMoving(Movement: EnemyProjectileFrameBehavior.TourianStatueRevealFollow)),
        ],
        [P.BaseDecorationLoop] =
        [
            Show(25, 1911),
        ],
        [P.Ridley] =
        [
            Show(26, 1911),
        ],
        [P.Phantoon] =
        [
            Show(27, 1911),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<TourianStatueProjectileFrame, TourianStatueProjectileCue> Show(int pose, int duration) => new AnimationBeat<TourianStatueProjectileFrame, TourianStatueProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<TourianStatueProjectileFrame, TourianStatueProjectileCue> Cue(TourianStatueProjectileCue cue) => new AnimationBeat<TourianStatueProjectileFrame, TourianStatueProjectileCue>.Event(cue);
}
