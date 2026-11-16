namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The authored colour fades of Mother Brain's encounter (#2270), each shown one image a step from its own progress
/// (<see cref="MotherBrainFadeProgress"/>) and completed on the step after its last image.
/// </summary>
public static class MotherBrainFadeDefinitions
{
    /// <summary>Her brain's three fading colours greying as she fakes her death.</summary>
    public static MotherBrainFade FakeDeathToGrey { get; } = new("mother-brain-fake-death-to-grey", MotherBrainFakeDeathFadeDefinitions.FrameCount);

    /// <summary>Her brain's three fading colours returning from the fake death's grey.</summary>
    public static MotherBrainFade FakeDeathFromGrey { get; } = new("mother-brain-fake-death-from-grey", MotherBrainFakeDeathFadeDefinitions.FrameCount);

    /// <summary>Her body, brain and drained rear legs greying as the Baby drains her.</summary>
    public static MotherBrainFade DrainToGrey { get; } = new("mother-brain-to-grey", 8);

    /// <summary>Her body, brain and drained rear legs reviving from the drain's grey.</summary>
    public static MotherBrainFade RevivalFromGrey { get; } = new("mother-brain-from-grey", 8);

    /// <summary>Her body, brain and rear legs blackening as she dies; the step after its last hides her body.</summary>
    public static MotherBrainFade DeathBody { get; } = new("mother-brain-death-body", 16);

    /// <summary>Her detached brain, in the corpse's colours, greying as it becomes her corpse.</summary>
    public static MotherBrainFade CorpseFade { get; } = new("mother-brain-corpse-fade", 8);

    /// <summary>The Baby fading to black as it dies.</summary>
    public static MotherBrainFade BabyFade { get; } = new("mother-brain-baby-fade", BabyMetroidCutsceneColorDefinitions.FadeFrameCount);
}
