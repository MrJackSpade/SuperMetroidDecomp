using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The pictures a sheet holds of a frame encounter artwork sequences change (#1513): one for each combination of the
/// distinct pictures each sequence gives it, the loaded picture among them.
/// </summary>
internal static class FrameVariationPictures
{
    /// <summary>
    /// Each combination of the steps whose pictures <paramref name="variations"/> show, each a list of the sequences
    /// showing a step's picture and that step; the empty combination is the loaded picture.
    /// </summary>
    internal static IEnumerable<IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)>> Combinations(IReadOnlyList<FrameArtworkVariation> variations)
    {
        IEnumerable<IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)>> combinations = [[]];
        foreach (FrameArtworkVariation variation in Ensure.NotNull(variations))
        {
            int[] steps = [.. variation.Pictures.OfType<int>().Distinct().Order()];
            combinations = [.. combinations.SelectMany(earlier => steps.Select(step =>
                (IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)>)[.. earlier, (variation.Sequence, step)]).Prepend(earlier))];
        }
        return combinations;
    }

    /// <summary>The names of the pictures a sheet holds of <paramref name="frame"/>, which <paramref name="variations"/> change.</summary>
    internal static IEnumerable<string> Names(string frame, IReadOnlyList<FrameArtworkVariation> variations) =>
        Combinations(variations).Select(combination => FrameArtworkVariation.PictureName(frame, combination));
}
