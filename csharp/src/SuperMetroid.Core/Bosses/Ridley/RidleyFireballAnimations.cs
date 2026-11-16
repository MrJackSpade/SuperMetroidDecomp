using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using C = SuperMetroid.Core.Bosses.Ridley.RidleyFireballCue;
using P = SuperMetroid.Core.Bosses.Ridley.RidleyFireballSegment;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's typed fireball and afterburn animations (#641, #1600, #2011), each beat the installed
/// <see cref="EnemyProjectileFrameId"/> it shows.
/// </summary>
internal static class RidleyFireballAnimations
{
    /// <summary>
    /// Every fireball and afterburn segment's beats: the fireball stops, shows its two four-update launch poses and starts
    /// flying between them, then loops four two-update flight poses. Each afterburn run stops moving, shows five five-update
    /// poses, raising the spawn of its next afterburns after the first, and deletes itself.
    /// </summary>
    internal static AnimationSegments<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments { get; } = new(
        new Dictionary<RidleyFireballSegment, AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>[]>
    {
        [P.Fireball] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.RidleyFireballTurned0, 4),
            Cue(C.BeginFlight),
            Show(EnemyProjectileFrameId.RidleyFireballTurned0Twin, 4),
            Show(EnemyProjectileFrameId.RidleyFireballTurned0TwinB, 2),
            Show(EnemyProjectileFrameId.RidleyFireballTurned90, 2),
            Show(EnemyProjectileFrameId.RidleyFireballTurned180, 2),
            Show(EnemyProjectileFrameId.RidleyFireballTurned270, 2),
        ],
        [P.AfterburnFinal] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.RidleyFireballSpark, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloud, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudSpreading, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudBreaking, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudEmbers, 5),
            Cue(C.Delete),
        ],
        [P.HorizontalCenter] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.RidleyFireballSparkTwin, 5),
            Cue(C.SpawnHorizontalAfterburns),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudTwin, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudSpreadingTwin, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudBreakingTwin, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudEmbersTwin, 5),
            Cue(C.Delete),
        ],
        [P.VerticalCenter] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.RidleyFireballSparkTwinB, 5),
            Cue(C.SpawnVerticalAfterburns),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudTwinB, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudSpreadingTwinB, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudBreakingTwinB, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudEmbersTwinB, 5),
            Cue(C.Delete),
        ],
        [P.DirectionalAfterburn] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.RidleyFireballSparkTwinC, 5),
            Cue(C.SpawnNextAfterburn),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudTwinC, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudSpreadingTwinC, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudBreakingTwinC, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudEmbersTwinC, 5),
            Cue(C.Delete),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment: the launch runs on into the flight loop, and each afterburn run holds
    /// after its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>> StartingAt { get; } =
        new Dictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>>
        {
            [P.Fireball] = new([new(P.Fireball, 0, 4, 1), new(P.Fireball, 4, 4, 1)], AnimationEnd.LoopsFrom(1)),
            [P.AfterburnFinal] = new([Whole(P.AfterburnFinal)], AnimationEnd.Holds),
            [P.HorizontalCenter] = new([Whole(P.HorizontalCenter)], AnimationEnd.Holds),
            [P.VerticalCenter] = new([Whole(P.VerticalCenter)], AnimationEnd.Holds),
            [P.DirectionalAfterburn] = new([Whole(P.DirectionalAfterburn)], AnimationEnd.Holds),
        };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RidleyFireballSegment> StartingWith(RidleyFireballSegment segment) => StartingAt[segment];

    /// <summary>A phase playing every beat of <paramref name="segment"/> once.</summary>
    private static AnimationPhase<RidleyFireballSegment> Whole(RidleyFireballSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue> Cue(RidleyFireballCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Event(cue);
}
