using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Shitroid's typed animations (#641, #1600).</summary>
internal static class ShitroidAnimations
{
    /// <summary>Every Shitroid segment.</summary>
    internal static AnimationSegments<ShitroidSegment, BabyMetroidFrame, ShitroidCue> Segments { get; } = new(
        new Dictionary<ShitroidSegment, AnimationBeat<BabyMetroidFrame, ShitroidCue>[]>
    {
        [ShitroidSegment.FinishDraining] = [Show(BabyMetroidFrame.WideOpen, 128), Show(BabyMetroidFrame.HalfOpen, 16)],
        [ShitroidSegment.Normal] = [Show(BabyMetroidFrame.Closed, 16), Show(BabyMetroidFrame.HalfOpen, 16), Show(BabyMetroidFrame.WideOpen, 16), Show(BabyMetroidFrame.HalfOpen, 16)],
        [ShitroidSegment.LatchedOn] = [Show(BabyMetroidFrame.Closed, 8), Show(BabyMetroidFrame.HalfOpen, 8), Show(BabyMetroidFrame.WideOpen, 5), Show(BabyMetroidFrame.HalfOpen, 2)],
        [ShitroidSegment.Remorse] = Remorse(),
    });

    /// <summary>
    /// The animation that starts at each segment: the pulses cycle for good, and the end of draining runs on into the
    /// ordinary pulse.
    /// </summary>
    internal static IReadOnlyDictionary<ShitroidSegment, Animation<ShitroidSegment>> StartingAt { get; } = new Dictionary<ShitroidSegment, Animation<ShitroidSegment>>
    {
        [ShitroidSegment.FinishDraining] = new([Whole(ShitroidSegment.FinishDraining), Whole(ShitroidSegment.Normal)], AnimationEnd.LoopsFrom(1)),
        [ShitroidSegment.Normal] = new([Whole(ShitroidSegment.Normal)], AnimationEnd.LoopsFrom(0)),
        [ShitroidSegment.LatchedOn] = new([Whole(ShitroidSegment.LatchedOn)], AnimationEnd.LoopsFrom(0)),
        [ShitroidSegment.Remorse] = new([Whole(ShitroidSegment.Remorse)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<ShitroidSegment> Starting(ShitroidSegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<ShitroidSegment> Whole(ShitroidSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>
    /// Two pulses at ten updates a pose, the random cry, then one pulse that quickens from six updates a pose to two
    /// and slows again to nine.
    /// </summary>
    private static AnimationBeat<BabyMetroidFrame, ShitroidCue>[] Remorse()
    {
        var beats = new List<AnimationBeat<BabyMetroidFrame, ShitroidCue>>();
        for (int pose = 0; pose < 8; pose++)
            beats.Add(Show(PulsePose(pose), 10));
        beats.Add(new AnimationBeat<BabyMetroidFrame, ShitroidCue>.Event(ShitroidCue.MaybeRemorseCry));
        for (int pose = 0; pose < 12; pose++)
            beats.Add(Show(PulsePose(pose), 2 + Math.Abs(pose - 4)));
        return [.. beats];
    }

    /// <summary>The pulse's pose order: closed, half open, wide open, half open.</summary>
    private static BabyMetroidFrame[] Pulse =>
        [BabyMetroidFrame.Closed, BabyMetroidFrame.HalfOpen, BabyMetroidFrame.WideOpen, BabyMetroidFrame.HalfOpen];

    /// <summary>The pulse's pose at <paramref name="step"/>, repeating every four steps.</summary>
    private static BabyMetroidFrame PulsePose(int step) => Pulse[step % Pulse.Length];

    /// <summary>Shows pose <paramref name="pose"/>.</summary>
    private static AnimationBeat<BabyMetroidFrame, ShitroidCue> Show(BabyMetroidFrame pose, int duration) =>
        new AnimationBeat<BabyMetroidFrame, ShitroidCue>.Frame(pose, duration);
}
