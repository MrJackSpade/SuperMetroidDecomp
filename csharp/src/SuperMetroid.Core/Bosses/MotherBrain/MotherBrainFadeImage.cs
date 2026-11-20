namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One image of one of Mother Brain's encounter's authored colour fades (#2270).</summary>
public readonly record struct MotherBrainFadeImage
{
    /// <summary>The image at <paramref name="step"/> of <paramref name="fade"/>.</summary>
    internal MotherBrainFadeImage(MotherBrainFade fade, int step)
    {
        Fade = fade;
        Step = step;
    }

    /// <summary>The fade the image belongs to.</summary>
    public MotherBrainFade Fade { get; }

    /// <summary>The image's place in its fade, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Fade} {Step}";
}
