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
            Show(EnemyProjectileFrameId.CeresRidleyFireballTurned0, 4),
            Cue(C.BeginFlight),
            Show(EnemyProjectileFrameId.CeresRidleyFireballTurned0Twin, 4),
            Show(EnemyProjectileFrameId.CeresRidleyFireballTurned0TwinB, 2),
            Show(EnemyProjectileFrameId.CeresRidleyFireballTurned90, 2),
            Show(EnemyProjectileFrameId.CeresRidleyFireballTurned180, 2),
            Show(EnemyProjectileFrameId.CeresRidleyFireballTurned270, 2),
        ],
        [P.AfterburnFinal] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.CeresRidleyFireballSpark, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloud, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreading, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreaking, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbers, 5),
            Cue(C.Delete),
        ],
        [P.HorizontalCenter] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.CeresRidleyFireballSparkTwin, 5),
            Cue(C.SpawnHorizontalAfterburns),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudTwin, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreadingTwin, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreakingTwin, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbersTwin, 5),
            Cue(C.Delete),
        ],
        [P.VerticalCenter] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.CeresRidleyFireballSparkTwinB, 5),
            Cue(C.SpawnVerticalAfterburns),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudTwinB, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreadingTwinB, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreakingTwinB, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbersTwinB, 5),
            Cue(C.Delete),
        ],
        [P.DirectionalAfterburn] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.CeresRidleyFireballSparkTwinC, 5),
            Cue(C.SpawnNextAfterburn),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudTwinC, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreadingTwinC, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreakingTwinC, 5),
            Show(EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbersTwinC, 5),
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
