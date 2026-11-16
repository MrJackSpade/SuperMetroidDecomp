namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the authored colour fades of Mother Brain's encounter (#2270): a run of installed images shown one a step, in
/// order. Each image is a colouring of its own that the import reads from the game's tables, not a blend of the fade's
/// ends, so a fade is its images rather than a typed blend.
/// </summary>
public sealed class MotherBrainFade
{
    /// <summary>Defines a fade.</summary>
    /// <param name="key">The name of its installed shades.</param>
    /// <param name="imageCount">The images it shows, one a step.</param>
    internal MotherBrainFade(string key, int imageCount)
    {
        Key = Ensure.NotNull(key);
        Images = [.. Enumerable.Range(0, Ensure.BetweenInclusive(1, int.MaxValue, imageCount)).Select(step => new MotherBrainFadeImage(this, step))];
    }

    /// <summary>The name of its installed shades.</summary>
    public string Key { get; }

    /// <summary>Its images, in the order it shows them.</summary>
    public IReadOnlyList<MotherBrainFadeImage> Images { get; }

    /// <summary>
    /// The image the fade shows at its step <paramref name="step"/>, counting from zero, or null at the step after its
    /// last, which shows no new colours and completes the fade.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is negative.</exception>
    internal MotherBrainFadeImage? ImageAt(int step) => Ensure.AtLeastZero(step) < Images.Count ? Images[step] : null;

    /// <inheritdoc/>
    public override string ToString() => Key;
}
