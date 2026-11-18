namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One image of Mother Brain's rainbow colour cycle (#2259), which her beam steps through over her body, brain and rear
/// legs.
/// </summary>
public readonly record struct MotherBrainRainbowImage
{
    /// <summary>The image at <paramref name="step"/> of the cycle.</summary>
    private MotherBrainRainbowImage(int step) => Step = step;

    /// <summary>The image's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>
    /// The image Mother Brain is left showing when the Baby Metroid exhausts the drain (#2470): the first of her second
    /// phase's death colours, which the cycle's last images are.
    /// </summary>
    public static MotherBrainRainbowImage Drained { get; } = new(Artwork.MotherBrainRainbowColourFormat.SecondPhaseDeathFrame);

    /// <summary>Every image of the cycle, in order: the ten the rainbow steps through.</summary>
    public static IReadOnlyList<MotherBrainRainbowImage> All { get; } = [.. Enumerable.Range(0, Artwork.MotherBrainRainbowColourFormat.RainbowFrameCount).Select(step => new MotherBrainRainbowImage(step))];

    /// <summary>
    /// The cycle at its first image, where her beam's colours start and start again (#2470): each of the rainbow's colour
    /// steps shows the image the cycle stands at and moves it on to the next, the first after the last.
    /// </summary>
    public static ListStep<MotherBrainRainbowImage> CycleStart => ListStep<MotherBrainRainbowImage>.FirstOf(All);
}
