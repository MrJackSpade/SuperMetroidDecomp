namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the authored colour runs of the Ridley encounters (#2316): a run of installed images their owner shows one at
/// a time, as a fade, a reveal, a zoom's shades or a cycling alarm. Each image is a colouring of its own that the import
/// supplies, not a blend of the run's ends, so a run is its images rather than a typed blend.
/// </summary>
public sealed class RidleyFade
{
    /// <summary>Defines a run.</summary>
    /// <param name="key">The name of its installed shades.</param>
    /// <param name="imageCount">The images it holds.</param>
    internal RidleyFade(string key, int imageCount)
    {
        Key = Ensure.NotNull(key);
        Images = [.. Enumerable.Range(0, Ensure.BetweenInclusive(1, int.MaxValue, imageCount)).Select(step => new RidleyFadeImage(this, step))];
    }

    /// <summary>The name of its installed shades.</summary>
    public string Key { get; }

    /// <summary>Its images, in their installed order.</summary>
    public IReadOnlyList<RidleyFadeImage> Images { get; }

    /// <summary>
    /// The image at <paramref name="step"/> of the run, counting from zero, or null at the step after its last, which
    /// shows no new colours.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is negative.</exception>
    internal RidleyFadeImage? ImageAt(int step) => Ensure.AtLeastZero(step) < Images.Count ? Images[step] : null;

    /// <inheritdoc/>
    public override string ToString() => Key;
}
