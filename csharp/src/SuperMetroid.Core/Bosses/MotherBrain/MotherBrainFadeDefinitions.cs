namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The authored colour fades of Mother Brain's encounter (#2270), each its images held for their hold and played through
/// the shared animation player from its own progress (<see cref="MotherBrainFadeProgress"/>, #2404), over once its last
/// image has shown its hold out.
/// </summary>
public static class MotherBrainFadeDefinitions
{
    /// <summary>Her brain's three fading colours greying as she fakes her death.</summary>
    public static MotherBrainFade FakeDeathToGrey { get; } = new("mother-brain-fake-death-to-grey", MotherBrainFakeDeathFadeDefinitions.FrameCount,
        MotherBrainWaitDefinitions.FakeDeathGreyStepUpdates);

    /// <summary>Her brain's three fading colours returning from the fake death's grey.</summary>
    public static MotherBrainFade FakeDeathFromGrey { get; } = new("mother-brain-fake-death-from-grey", MotherBrainFakeDeathFadeDefinitions.FrameCount,
        MotherBrainWaitDefinitions.FromGreyStepUpdates);

    /// <summary>Her body, brain and drained rear legs greying as the Baby drains her.</summary>
    public static MotherBrainFade DrainToGrey { get; } = new("mother-brain-to-grey", 8, MotherBrainFightDefinitions.GreyStepUpdates);

    /// <summary>Her body, brain and drained rear legs reviving from the drain's grey.</summary>
    public static MotherBrainFade RevivalFromGrey { get; } = new("mother-brain-from-grey", 8, MotherBrainFightDefinitions.GreyStepUpdates);

    /// <summary>Her body, brain and rear legs blackening as she dies; the update it is over hides her body.</summary>
    public static MotherBrainFade DeathBody { get; } = new("mother-brain-death-body", 16, MotherBrainLateFightDefinitions.BodyFadeStepUpdates);

    /// <summary>Her detached brain, in the corpse's colours, greying as it becomes her corpse.</summary>
    public static MotherBrainFade CorpseFade { get; } = new("mother-brain-corpse-fade", 8, MotherBrainFightDefinitions.GreyStepUpdates);

    /// <summary>Her room's lights recovering after the Baby's death (#2316), an image an update; the update it is over ends the cutscene.</summary>
    public static MotherBrainFade RecoveryLights { get; } = new("mother-brain-recovery-lights", MotherBrainRoomColorDefinitions.RecoveryLightsFrames, 1);

    /// <summary>The Baby fading to black as it dies.</summary>
    public static MotherBrainFade BabyFade { get; } = new("mother-brain-baby-fade", BabyMetroidCutsceneColorDefinitions.FadeFrameCount,
        BabyMetroidCutsceneDefinitions.FadeToBlackStepUpdates);
}
