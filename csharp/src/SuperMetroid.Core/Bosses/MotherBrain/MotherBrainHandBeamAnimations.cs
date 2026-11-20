using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The red hand beam's typed animation (#641, #1600, #2011), each beat the installed <see cref="EnemyProjectileFrameId"/> it
/// shows.
/// </summary>
internal static class MotherBrainHandBeamAnimations
{
    /// <summary>How many stages the beam plays before it deletes itself.</summary>
    private const int Stages = 3;

    /// <summary>
    /// The beam's stage, spawning the next beam after its first frame and fading over its frames, each held shorter than
    /// the last, and its deletion.
    /// </summary>
    internal static AnimationSegments<MotherBrainHandBeamSegment, EnemyProjectileFrameId, MotherBrainHandBeamCue> Segments { get; } = new(
        new Dictionary<MotherBrainHandBeamSegment, Beat[]>
        {
            [MotherBrainHandBeamSegment.Stage] =
            [
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbSmall, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRed, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCore, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbOrange, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCore, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbYellow, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhite, 1),
            ],
            [MotherBrainHandBeamSegment.Delete] = [new Beat.Event(MotherBrainHandBeamCue.Delete)],
        });

    /// <summary>
    /// The beam's animation: its stages (<see cref="Stages"/>) and its deletion, played once; the deleted beam then rests. The charging beam
    /// and every fired beam play it.
    /// </summary>
    internal static IReadOnlyDictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>> StartingAt { get; } =
        new Dictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>>
        {
            [MotherBrainHandBeamSegment.Stage] = new(
                [Segments.Repeated(MotherBrainHandBeamSegment.Stage, Stages), Segments.Whole(MotherBrainHandBeamSegment.Delete)], AnimationEnd.Holds),
        };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static Beat Show(EnemyProjectileFrameId frame, int duration) => new Beat.Frame(frame, duration);
}
