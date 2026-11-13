using SuperMetroid.Core.Animation;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamFrame, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The red hand beam's typed animation (#641, #1600).</summary>
internal static class MotherBrainHandBeamAnimations
{
    /// <summary>Stages the beam runs before deleting itself.</summary>
    internal const int StageCount = 3;

    /// <summary>
    /// Three stages, each spawning the next beam after its first frame and fading over seven frames held 3, 3, 2, 2,
    /// 1, 1 and 1 updates, then the deletion. The charging beam and every fired beam play it.
    /// </summary>
    internal static AnimationSegments<MotherBrainHandBeamSegment, MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Segments { get; } = new(
        new Dictionary<MotherBrainHandBeamSegment, Beat[]>
        {
            [MotherBrainHandBeamSegment.Beam] =
            [
                .. Enumerable.Range(0, StageCount).SelectMany(Stage),
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

    /// <summary>The beats of stage <paramref name="stage"/>.</summary>
    private static IEnumerable<Beat> Stage(int stage)
    {
        yield return new Beat.Frame(new(stage, 0), 3);
        yield return new Beat.Event(MotherBrainHandBeamCue.SpawnNextBeam);
        foreach ((int phase, int duration) in new[] { new MotherBrainHandBeamFrameHold(1, 3), new MotherBrainHandBeamFrameHold(2, 2), new MotherBrainHandBeamFrameHold(3, 2), new MotherBrainHandBeamFrameHold(4, 1), new MotherBrainHandBeamFrameHold(5, 1), new MotherBrainHandBeamFrameHold(6, 1) })
            yield return new Beat.Frame(new(stage, phase), duration);
    }
}
