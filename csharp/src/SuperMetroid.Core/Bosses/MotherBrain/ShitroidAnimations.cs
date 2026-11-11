using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Shitroid's typed animations (#641, #1600).</summary>
internal static class ShitroidAnimations
{
    /// <summary>Every Shitroid segment.</summary>
    internal static AnimationSegments<ShitroidProgram, BabyMetroidFrame, ShitroidCue> Segments { get; } = new(
        new Dictionary<ShitroidProgram, AnimationBeat<BabyMetroidFrame, ShitroidCue>[]>
    {
        [ShitroidProgram.FinishDraining] = [Show(2, 128), Show(1, 16)],
        [ShitroidProgram.Normal] = [Show(0, 16), Show(1, 16), Show(2, 16), Show(1, 16)],
        [ShitroidProgram.LatchedOn] = [Show(0, 8), Show(1, 8), Show(2, 5), Show(1, 2)],
        [ShitroidProgram.Remorse] = Remorse(),
    });

    /// <summary>
    /// The animation that starts at each segment: the pulses cycle for good, and the end of draining runs on into the
    /// ordinary pulse.
    /// </summary>
    internal static IReadOnlyDictionary<ShitroidProgram, Animation<ShitroidProgram>> StartingAt { get; } = new Dictionary<ShitroidProgram, Animation<ShitroidProgram>>
    {
        [ShitroidProgram.FinishDraining] = new([Whole(ShitroidProgram.FinishDraining), Whole(ShitroidProgram.Normal)], AnimationEnd.LoopsFrom(1)),
        [ShitroidProgram.Normal] = new([Whole(ShitroidProgram.Normal)], AnimationEnd.LoopsFrom(0)),
        [ShitroidProgram.LatchedOn] = new([Whole(ShitroidProgram.LatchedOn)], AnimationEnd.LoopsFrom(0)),
        [ShitroidProgram.Remorse] = new([Whole(ShitroidProgram.Remorse)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="program"/>, before its first play.</summary>
    internal static AnimationPlayback<ShitroidProgram> Starting(ShitroidProgram program) =>
        new(StartingAt[program], AnimationPlace.StartOfAnimation);

    /// <summary>One play of every beat of <paramref name="program"/>.</summary>
    private static AnimationPhase<ShitroidProgram> Whole(ShitroidProgram program) => new(program, 0, Segments.Beats(program).Count, 1);

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
    private static int PulsePose(int step) => (step % 4) switch
    {
        3 => 1,
        int pose => pose,
    };

    /// <summary>Shows pose <paramref name="index"/>.</summary>
    private static AnimationBeat<BabyMetroidFrame, ShitroidCue> Show(int index, int duration) =>
        new AnimationBeat<BabyMetroidFrame, ShitroidCue>.Frame(new BabyMetroidFrame(index), duration);
}
