using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the authored colour fades of Mother Brain's encounter (#2270): a run of installed images, each held for the
/// fade's hold and shown in order, played through the shared animation player (#2404). Each image is a colouring of its own
/// that the import supplies, not a blend of the fade's ends, so a fade is its images rather than a typed blend. The fade is
/// over once its last image has shown its hold out.
/// </summary>
public sealed partial class MotherBrainFade
{
    /// <summary>Defines a fade.</summary>
    /// <param name="key">The name of its installed shades.</param>
    /// <param name="imageCount">The images it shows, in order.</param>
    /// <param name="imageUpdates">The updates each image shows for before the next, or before the fade is over.</param>
    /// <param name="layer">The colouring of the encounter its images show on.</param>
    internal MotherBrainFade([InstalledKey] string key, int imageCount, int imageUpdates, MotherBrainFadeLayer layer)
    {
        Key = Ensure.NotNull(key);
        Layer = layer;
        Images = [.. Enumerable.Range(0, Ensure.BetweenInclusive(1, int.MaxValue, imageCount)).Select(step => new MotherBrainFadeImage(this, step))];
        ImageUpdates = Ensure.BetweenInclusive(1, int.MaxValue, imageUpdates);
        Segments = new(new Dictionary<MotherBrainFadeSegment, AnimationBeat<MotherBrainFadeImage, NoAnimationCue>[]>
        {
            [MotherBrainFadeSegment.Images] = [.. Images.Select(image => new AnimationBeat<MotherBrainFadeImage, NoAnimationCue>.Frame(image, imageUpdates))],
        });
        Animation = new([Segments.Whole(MotherBrainFadeSegment.Images)], AnimationEnd.Stops);
    }

    /// <summary>The name of its installed shades.</summary>
    public string Key { get; }

    /// <summary>The colouring of the encounter its images show on.</summary>
    public MotherBrainFadeLayer Layer { get; }

    /// <summary>Its images, in the order it shows them.</summary>
    public IReadOnlyList<MotherBrainFadeImage> Images { get; }

    /// <summary>The updates each image shows for.</summary>
    public int ImageUpdates { get; }

    /// <summary>The fade's run of images.</summary>
    internal AnimationSegments<MotherBrainFadeSegment, MotherBrainFadeImage, NoAnimationCue> Segments { get; }

    /// <summary>The fade played once: its images in order, and then it is over.</summary>
    internal Animation<MotherBrainFadeSegment> Animation { get; }

    /// <inheritdoc/>
    public override string ToString() => Key;
}
