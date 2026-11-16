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
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbSmall, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRed, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCore, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbOrange, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCore, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbYellow, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhite, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbSmallTwin, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRedTwin, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCoreTwin, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbOrangeTwin, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCoreTwin, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbYellowTwin, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteTwin, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbSmallTwinB, 3),
                new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRedTwinB, 3),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCoreTwinB, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbOrangeTwinB, 2),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCoreTwinB, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbYellowTwinB, 1),
                Show(EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteTwinB, 1),
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
