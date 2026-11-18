using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Hibashi.HibashiCueKind;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's erupting flame and its collision part as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HibashiCue"/> with the values it acts on.
/// </summary>
internal static class HibashiAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HibashiSegment, EnemyFrameId, HibashiCue> Segments { get; } = new(
        new Dictionary<HibashiSegment, AnimationBeat<EnemyFrameId, HibashiCue>[]>
    {
        [P.Flame] =
        [
            Cue(new HibashiCue.Signal(K.PlayEruptionSound)),
            Show(EnemyFrameId.HibashiEmber, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 5, YRadius: 24, HibashiStepWidth.Restored))),
            Show(EnemyFrameId.HibashiFlame19By19, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 10, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame23By26, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 15, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame33By35, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 20, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame37By41, 1),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 25, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame19By43, 1),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 30, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame23By45, 1),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 35, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame33By47, 1),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 40, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame37By55, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 45, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame19By63, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 50, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame23By71, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 55, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame33By80, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 60, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame41By87, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 65, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame45By95, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 70, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame17By99, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 75, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame19By109, 2),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 80, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame23By112, 4),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 85, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame33By109, 4),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 90, YRadius: 24))),
            Show(EnemyFrameId.HibashiFlame37By111, 4),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 95, YRadius: 20))),
            Show(EnemyFrameId.HibashiDispersing17By44, 4),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 100, YRadius: 16))),
            Show(EnemyFrameId.HibashiDispersing17By32, 4),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 105, YRadius: 12))),
            Show(EnemyFrameId.HibashiDispersing17By25, 4),
            Cue(new HibashiCue.EruptionStep(new(YOffset: 110, YRadius: 8))),
            Show(EnemyFrameId.HibashiDispersing14By16, 4),
            Cue(new HibashiCue.Signal(K.FinishActivity)),
        ],
        [P.Hitbox] =
        [
            Hold(EnemyFrameId.HibashiEmber),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<HibashiSegment, Animation<HibashiSegment>> StartingAt { get; } = new Dictionary<HibashiSegment, Animation<HibashiSegment>>
    {
        [P.Flame] = new([Segments.Whole(P.Flame)], AnimationEnd.Holds),
        [P.Hitbox] = new([Segments.Whole(P.Hitbox)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HibashiSegment> StartingWith(HibashiSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HibashiCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): the collision part's still picture rests on it.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, HibashiCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Cue(HibashiCue cue) => new AnimationBeat<EnemyFrameId, HibashiCue>.Event(cue);
}
