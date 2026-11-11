using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoTourianDustProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep and explosion dust and the Tourian statue's descent dust (#641). Each
/// frame shows a <see cref="ChozoTourianDustFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class ChozoTourianDustAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<ChozoTourianDustProgram, AnimationBeat<ChozoTourianDustFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Footsteps] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 3)))),
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SpikeClearingExplosions] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 3)))),
            Show(4, 5),
            Show(5, 5),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.TourianDescentDust] =
        [
        ],
        [P.TourianDescentDustLoop] =
        [
            Cue(new EnemyProjectileCommonCue.Command(CK.ReturnToOrigin)),
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 6), YRange: new(0, 2), Center: new(0, 0)))),
            Show(10, 2),
            Show(11, 2),
            Show(12, 2),
            Show(13, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChozoTourianDustProgram, Animation<ChozoTourianDustProgram>> StartingAt { get; } = new Dictionary<ChozoTourianDustProgram, Animation<ChozoTourianDustProgram>>
    {
        [P.Footsteps] = new([new(P.Footsteps, 0, 6, 1)], AnimationEnd.Holds),
        [P.SpikeClearingExplosions] = new([new(P.SpikeClearingExplosions, 0, 8, 1)], AnimationEnd.Holds),
        [P.TourianDescentDust] = new([new(P.TourianDescentDustLoop, 0, 6, 64), new(P.TourianDescentDustLoop, 6, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoTourianDustProgram> StartingWith(ChozoTourianDustProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ChozoTourianDustFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<ChozoTourianDustFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<ChozoTourianDustFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<ChozoTourianDustFrame, EnemyProjectileCommonCue>.Event(cue);
}
