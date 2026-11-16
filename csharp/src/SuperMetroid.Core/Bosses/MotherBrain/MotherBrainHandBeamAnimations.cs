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

    /// <summary>
    /// Three stages, each spawning the next beam after its first frame and fading over seven frames held 3, 3, 2, 2,
    /// 1, 1 and 1 updates, then the deletion. The charging beam and every fired beam play it.
    /// </summary>
    internal static AnimationSegments<MotherBrainHandBeamSegment, EnemyProjectileFrameId, MotherBrainHandBeamCue> Segments { get; } = new(
        new Dictionary<MotherBrainHandBeamSegment, Beat[]>
        {
            [MotherBrainHandBeamSegment.Beam] =
            [
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageFirstPhase, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageSecondPhase, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageThirdPhase, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageFourthPhase, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageFifthPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageSixthPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamFirstStageSeventhPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageFirstPhase, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageSecondPhase, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageThirdPhase, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageFourthPhase, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageFifthPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageSixthPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamSecondStageSeventhPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageFirstPhase, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageSecondPhase, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageThirdPhase, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageFourthPhase, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageFifthPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageSixthPhase, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamThirdStageSeventhPhase, 1),
                new Beat.Event(MotherBrainHandBeamCue.Delete),
            ],
        });

    /// <summary>The beam's animation: its stages and deletion, played once; the deleted beam then rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>> StartingAt { get; } =
        new Dictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>>
        {
            [MotherBrainHandBeamSegment.Beam] = new(
                [new(MotherBrainHandBeamSegment.Beam, 0, Segments.Beats(MotherBrainHandBeamSegment.Beam).Count, 1)], AnimationEnd.Holds),
        };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static Beat Show(EnemyProjectileFrameId frame, int duration) => new Beat.Frame(frame, duration);
}
