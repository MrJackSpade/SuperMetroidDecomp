namespace SuperMetroid.Core.Game;

/// <summary>The Tourian Shitroid's programs, in bank $A9 order.</summary>
public enum ShitroidProgram
{
    /// <summary><c>InstList_BabyMetroid_FinishDraining</c> (<c>$A9:F906</c>): a long wide-open hold, then calm.</summary>
    FinishDraining,
    /// <summary><c>InstList_BabyMetroid_Normal</c> (<c>$A9:F90E</c>): the calm pulse.</summary>
    Normal,
    /// <summary><c>InstList_BabyMetroid_LatchedOn</c> (<c>$A9:F924</c>): the quick draining pulse.</summary>
    LatchedOn,
    /// <summary><c>InstList_BabyMetroid_Remorse</c> (<c>$A9:F93A</c>): the hesitant pulse before leaving.</summary>
    Remorse,
}

/// <summary>Gameplay cues of the Shitroid's programs.</summary>
internal enum ShitroidCue
{
    /// <summary>
    /// <c>$A9:F994</c> samples the random word: with bit 15 clear the remorse pulse starts over; with it set the
    /// Shitroid cries and runs its quickening and slowing pulse.
    /// </summary>
    MaybeRemorseCry,
}

/// <summary>The Shitroid's typed programs (#641), in place of its bank-$A9 instruction lists.</summary>
internal static class ShitroidAnimations
{
    /// <summary>Every Shitroid program.</summary>
    internal static AnimationProgramSet<ShitroidProgram, BabyMetroidFrame, ShitroidCue> Programs { get; } = new(
        new Dictionary<ShitroidProgram, AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>[]>
    {
        [ShitroidProgram.FinishDraining] = [Show(2, 128), Show(1, 16), FallInto(ShitroidProgram.Normal)],
        [ShitroidProgram.Normal] = [Show(0, 16), Show(1, 16), Show(2, 16), Show(1, 16), GoTo(ShitroidProgram.Normal)],
        [ShitroidProgram.LatchedOn] = [Show(0, 8), Show(1, 8), Show(2, 5), Show(1, 2), GoTo(ShitroidProgram.LatchedOn)],
        [ShitroidProgram.Remorse] = Remorse(),
    });

    /// <summary>
    /// Two pulses at ten updates a pose, the random cry, then one pulse that quickens from six updates a pose to two
    /// and slows again to nine.
    /// </summary>
    private static AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>[] Remorse()
    {
        var steps = new List<AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>>();
        for (int pose = 0; pose < 8; pose++)
            steps.Add(Show(PulsePose(pose), 10));
        steps.Add(AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.Publish(ShitroidCue.MaybeRemorseCry));
        for (int pose = 0; pose < 12; pose++)
            steps.Add(Show(PulsePose(pose), 2 + Math.Abs(pose - 4)));
        steps.Add(GoTo(ShitroidProgram.Remorse));
        return [.. steps];
    }

    /// <summary>The pulse's pose order: closed, half open, wide open, half open.</summary>
    private static int PulsePose(int step) => (step % 4) switch
    {
        3 => 1,
        int pose => pose,
    };

    /// <summary>Shows pose <paramref name="index"/>.</summary>
    private static AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue> Show(int index, int duration) =>
        AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.Show(new BabyMetroidFrame(index), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue> GoTo(ShitroidProgram program) =>
        AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue> FallInto(ShitroidProgram program) =>
        AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.FallInto(program);
}
