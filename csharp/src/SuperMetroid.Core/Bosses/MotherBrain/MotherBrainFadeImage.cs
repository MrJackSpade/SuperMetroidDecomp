namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One image of one of Mother Brain's encounter's authored colour fades (#2270).</summary>
public readonly partial record struct MotherBrainFadeImage
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

    /// <summary>
    /// This image, which <paramref name="expected"/>'s owner shows only from that fade.
    /// </summary>
    /// <exception cref="ArgumentException">The image belongs to another fade.</exception>
    internal MotherBrainFadeImage Of(MotherBrainFade expected) => ReferenceEquals(Fade, expected)
        ? this
        : throw new ArgumentException($"Image {Step} of {Fade} is not an image of {expected}.", nameof(expected));

    /// <inheritdoc/>
    public override string ToString() => $"{Fade} {Step}";
}
