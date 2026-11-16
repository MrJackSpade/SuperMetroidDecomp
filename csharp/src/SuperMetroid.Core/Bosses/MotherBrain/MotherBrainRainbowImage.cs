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

    /// <summary>The cycle's first image.</summary>
    public static MotherBrainRainbowImage First { get; } = new(0);

    /// <summary>The image Mother Brain is left showing when the Baby Metroid exhausts the drain.</summary>
    public static MotherBrainRainbowImage Drained { get; } = new(6);

    /// <summary>Every image of the cycle, in order: the ten the rainbow steps through.</summary>
    public static IReadOnlyList<MotherBrainRainbowImage> All { get; } = [.. Enumerable.Range(0, Artwork.MotherBrainRainbowColourFormat.RainbowFrameCount).Select(step => new MotherBrainRainbowImage(step))];

    /// <summary>The image after this one; the last is followed by the first.</summary>
    public MotherBrainRainbowImage Next => new((Step + 1) % Artwork.MotherBrainRainbowColourFormat.RainbowFrameCount);
}
