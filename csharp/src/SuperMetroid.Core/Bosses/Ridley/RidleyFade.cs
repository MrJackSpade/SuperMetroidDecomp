namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the authored colour runs of the Ridley encounters (#2316): a run of installed images their owner shows one at
/// a time, as a fade, a reveal, a zoom's shades or a cycling alarm. Each image is a colouring of its own that the import
/// supplies, not a blend of the run's ends, so a run is its images rather than a typed blend.
/// </summary>
public sealed partial class RidleyFade
{
    /// <summary>Defines a run.</summary>
    /// <param name="key">The name of its installed shades.</param>
    /// <param name="imageCount">The images it holds.</param>
    internal RidleyFade([InstalledKey] string key, int imageCount)
    {
        Key = Ensure.NotNull(key);
        Images = [.. Enumerable.Range(0, Ensure.BetweenInclusive(1, int.MaxValue, imageCount)).Select(step => new RidleyFadeImage(this, step))];
        End = Images[^1];
    }

    /// <summary>The name of its installed shades.</summary>
    public string Key { get; }

    /// <summary>Its images, in their installed order.</summary>
    public IReadOnlyList<RidleyFadeImage> Images { get; }

    /// <summary>The image the run ends on: a fade's full colours, which the encounter holds once the fade is over.</summary>
    public RidleyFadeImage End { get; }

    /// <summary>
    /// The image a quantity selects by its place in the run, counting from zero (#2409): the getaway floor's shade for its
    /// zoom's whole floor pixels per screen pixel.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The run has no image at <paramref name="step"/>.</exception>
    internal RidleyFadeImage ImageAt(int step) => Ensure.Element(Images, step);

    /// <inheritdoc/>
    public override string ToString() => Key;
}
