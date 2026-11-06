using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The pictures a sheet holds of a frame encounter artwork sequences change (#1513): one for each distinct combination of
/// the pictures its sequences give it at the steps they can reach together while it is drawn, the loaded picture among
/// them only when such a combination shows it. Sequences play independently of one another. A sequence that has not
/// reached its first step counts as drawn only when the frame is drawn before it.
/// </summary>
internal static class FrameVariationPictures
{
    /// <summary>
    /// Each distinct combination of the steps whose pictures <paramref name="variations"/> show while the frame is drawn,
    /// each a list of the sequences showing a step's picture and that step; the empty combination, the loaded picture, is
    /// among them when the frame shows it.
    /// </summary>
    internal static IEnumerable<IReadOnlyList<EncounterArtworkStep>> Combinations(IReadOnlyList<FrameArtworkVariation> variations)
    {
        Ensure.NotNull(variations);
        var named = new HashSet<string>(StringComparer.Ordinal);
        foreach (int?[] reached in ReachedSteps(variations))
        {
            EncounterArtworkStep[] shown = [.. variations.Select((variation, index) => Shown(variation, reached[index]))
                .OfType<EncounterArtworkStep>()];
            if (named.Add(FrameArtworkVariation.PictureName("frame", shown)))
                yield return shown;
        }
    }

    /// <summary>The names of the pictures a sheet holds of <paramref name="frame"/>, which <paramref name="variations"/> change.</summary>
    internal static IEnumerable<string> Names(string frame, IReadOnlyList<FrameArtworkVariation> variations) =>
        Combinations(variations).Select(combination => FrameArtworkVariation.PictureName(frame, combination));

    /// <summary>The picture <paramref name="variation"/> shows once its sequence has reached <paramref name="step"/>, if any.</summary>
    private static EncounterArtworkStep? Shown(FrameArtworkVariation variation, int? step) =>
        // Only steps the frame is drawn at are reached, each within the sequence.
        (step is int reached ? variation.Pictures[reached] : variation.PictureBeforeFirstStep) is int picture
            ? new EncounterArtworkStep(variation.Sequence, picture) : null;

    /// <summary>
    /// Each step, or none, the sequences <paramref name="variations"/> follow can have reached together, by variation:
    /// every step of each sequence the frame is drawn at with every step of the others.
    /// </summary>
    private static IEnumerable<int?[]> ReachedSteps(IReadOnlyList<FrameArtworkVariation> variations)
    {
        IEnumerable<int?[]> reached = [new int?[variations.Count]];
        for (int index = 0; index < variations.Count; index++)
        {
            int current = index;
            int?[] steps = [.. StepsOf(variations[current])];
            reached = [.. reached.SelectMany(earlier => steps.Select(step => With(earlier, current, step)))];
        }
        return reached;
    }

    /// <summary>Every step of <paramref name="variation"/>'s sequence the frame is drawn at, after none.</summary>
    private static IEnumerable<int?> StepsOf(FrameArtworkVariation variation) =>
        Enumerable.Range(0, variation.Pictures.Length).Select(step => (int?)step).Prepend(null).Where(step => Drawn(variation, step));

    /// <summary>
    /// Whether the frame <paramref name="variation"/> follows is drawn once its sequence has reached <paramref name="step"/>,
    /// or before its first step when none.
    /// </summary>
    private static bool Drawn(FrameArtworkVariation variation, int? step) =>
        step is int reached ? variation.DrawnAt(reached) : variation.DrawnBeforeFirstStep;

    /// <summary>A copy of <paramref name="reached"/> with variation <paramref name="index"/> at <paramref name="step"/>.</summary>
    private static int?[] With(int?[] reached, int index, int? step)
    {
        int?[] copy = [.. reached];
        copy[index] = step;
        return copy;
    }
}
