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
    /// flying between them, then runs on into its flight, four two-update poses (#2235). Each afterburn run stops moving and shows
    /// its five-update spark, raising the spawn of its next afterburns after it, then plays the puff every run shares (#2331):
    /// four five-update poses of its fire cloud, and deletes itself.
    /// </summary>
    internal static UnpausedAnimationSegments<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments { get; } = new(
        new Dictionary<RidleyFireballSegment, AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Unpaused[]>
    {
        [P.Fireball] =
        [
            Cue(C.StopMoving),
            Show(EnemyProjectileFrameId.RidleyFireballTurned0, 4),
            Cue(C.BeginFlight),
            Show(EnemyProjectileFrameId.RidleyFireballTurned0, 4),
        ],
        [P.FireballFlight] =
        [
            Show(EnemyProjectileFrameId.RidleyFireballTurned0, 2),
            Show(EnemyProjectileFrameId.RidleyFireballTurned90, 2),
            Show(EnemyProjectileFrameId.RidleyFireballTurned180, 2),
            Show(EnemyProjectileFrameId.RidleyFireballTurned270, 2),
        ],
        [P.AfterburnFinal] = AfterburnSpark,
        [P.HorizontalCenter] = [.. AfterburnSpark, Cue(C.SpawnHorizontalAfterburns)],
        [P.VerticalCenter] = [.. AfterburnSpark, Cue(C.SpawnVerticalAfterburns)],
        [P.DirectionalAfterburn] = [.. AfterburnSpark, Cue(C.SpawnNextAfterburn)],
        [P.AfterburnPuff] =
        [
            Show(EnemyProjectileFrameId.RidleyFireballFireCloud, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudSpreading, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudBreaking, 5),
            Show(EnemyProjectileFrameId.RidleyFireballFireCloudEmbers, 5),
            Cue(C.Delete),
        ],
    });

    /// <summary>Every afterburn run's opening: the fire stops where it is and sparks, before the run raises its spawn.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Unpaused[] AfterburnSpark =>
        [Cue(C.StopMoving), Show(EnemyProjectileFrameId.RidleyFireballSpark, 5)];

    /// <summary>
    /// The animation that starts at each segment: the launch runs on into the flight, which loops, and each afterburn run's
    /// spark runs on into the shared puff, which holds after its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>> StartingAt { get; } =
        new Dictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>>
        {
            [P.Fireball] = Animation<RidleyFireballSegment>.LeadingInto([Segments.Whole(P.Fireball)], [Segments.Whole(P.FireballFlight)]),
            [P.AfterburnFinal] = new([Segments.Whole(P.AfterburnFinal), Segments.Whole(P.AfterburnPuff)], AnimationEnd.Holds),
            [P.HorizontalCenter] = new([Segments.Whole(P.HorizontalCenter), Segments.Whole(P.AfterburnPuff)], AnimationEnd.Holds),
            [P.VerticalCenter] = new([Segments.Whole(P.VerticalCenter), Segments.Whole(P.AfterburnPuff)], AnimationEnd.Holds),
            [P.DirectionalAfterburn] = new([Segments.Whole(P.DirectionalAfterburn), Segments.Whole(P.AfterburnPuff)], AnimationEnd.Holds),
        };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Unpaused Cue(RidleyFireballCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Event(cue);
}
