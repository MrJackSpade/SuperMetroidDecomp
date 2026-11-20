using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBreathSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's purple breath animations (#641, #2554). Each frame shows a <see cref="EnemyProjectileFrameId"/> and each cue
/// is a <see cref="MotherBrainBreathCue"/>, which the breath keeping her encounter runs.
/// </summary>
internal static class MotherBrainBreathAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MotherBrainBreathSegment, EnemyProjectileFrameId, MotherBrainBreathCue> Segments { get; } = new(
        new Dictionary<MotherBrainBreathSegment, AnimationBeat<EnemyProjectileFrameId, MotherBrainBreathCue>[]>
    {
        [P.PurpleBreathBig] =
        [
            Cue(MotherBrainBreathCue.StopMoving),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePuff, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePlume, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePlumeLong, 9),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePlumeFull, 9),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeDispersing, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeThinning, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeFading, 11),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeFaint, 11),
            Cue(MotherBrainBreathCue.Delete),
        ],
        [P.PurpleBreathSmall] =
        [
            Cue(MotherBrainBreathCue.StopMoving),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallPuff, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallPuffGrowing, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallCloud, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallCloudLong, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallDispersing, 11),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallThinning, 11),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallFading, 12),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallFaint, 12),
            Cue(MotherBrainBreathCue.EndSmallPurpleBreath),
            Cue(MotherBrainBreathCue.Delete),
        ],
    });

    /// <summary>The animation that starts at each segment.</summary>
    internal static IReadOnlyDictionary<MotherBrainBreathSegment, Animation<MotherBrainBreathSegment>> StartingAt { get; } = new Dictionary<MotherBrainBreathSegment, Animation<MotherBrainBreathSegment>>
    {
        [P.PurpleBreathBig] = new([Segments.Whole(P.PurpleBreathBig)], AnimationEnd.Holds),
        [P.PurpleBreathSmall] = new([Segments.Whole(P.PurpleBreathSmall)], AnimationEnd.Holds),
    };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MotherBrainBreathCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, MotherBrainBreathCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MotherBrainBreathCue> Cue(MotherBrainBreathCue cue) => new AnimationBeat<EnemyProjectileFrameId, MotherBrainBreathCue>.Event(cue);
}
