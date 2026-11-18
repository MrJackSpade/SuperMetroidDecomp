namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One image of one of the Ridley encounters' authored colour runs (#2316).</summary>
public readonly record struct RidleyFadeImage
{
    /// <summary>The image at <paramref name="step"/> of <paramref name="fade"/>.</summary>
    internal RidleyFadeImage(RidleyFade fade, int step)
    {
        Fade = fade;
        Step = step;
    }

    /// <summary>The run the image belongs to.</summary>
    public RidleyFade Fade { get; }

    /// <summary>The image's place in its run, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>This image, which <paramref name="expected"/>'s owner shows only from that run.</summary>
    /// <exception cref="ArgumentException">The image belongs to another run.</exception>
    internal RidleyFadeImage Of(RidleyFade expected) => ReferenceEquals(Fade, expected)
        ? this
        : throw new ArgumentException($"Image {Step} of {Fade} is not an image of {expected}.", nameof(expected));

    /// <inheritdoc/>
    public override string ToString() => $"{Fade} {Step}";
}
