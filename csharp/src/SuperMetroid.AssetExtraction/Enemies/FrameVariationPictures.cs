using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The pictures a sheet holds of a frame encounter artwork sequences change (#1513): one for each distinct combination of
/// the pictures its sequences give it at the steps they can reach together, the loaded picture among them. Sequences play
/// independently of one another, except an interlude, which plays only while its main sequence holds one step.
/// </summary>
internal static class FrameVariationPictures
{
    /// <summary>
    /// Each distinct combination of the steps whose pictures <paramref name="variations"/> show, each a list of the
    /// sequences showing a step's picture and that step; the empty combination, the loaded picture, comes first.
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
        step is int reached && variation.PictureAt(reached) is int picture ? new EncounterArtworkStep(variation.Sequence, picture) : null;

    /// <summary>
    /// Each step, or none, the sequences <paramref name="variations"/> follow can have reached together, by variation:
    /// every step of each independent sequence with every step of the others, and an interlude's steps only with the step
    /// its main sequence holds while it plays.
    /// </summary>
    private static IEnumerable<int?[]> ReachedSteps(IReadOnlyList<FrameArtworkVariation> variations)
    {
        IEnumerable<int?[]> reached = [new int?[variations.Count]];
        var placed = new bool[variations.Count];
        for (int index = 0; index < variations.Count; index++)
        {
            if (placed[index])
                continue;
            placed[index] = true;
            EncounterArtworkSequence sequence = variations[index].Sequence;
            EncounterArtworkInterlude? interlude = EncounterArtworkInterludes.All.SingleOrDefault(entry => entry.Main == sequence || entry.Interlude == sequence);
            int partner = interlude is null ? -1
                : IndexOf(variations, interlude.Main == sequence ? interlude.Interlude : interlude.Main);
            if (interlude is null || partner < 0)
            {
                int current = index;
                int?[] steps = [.. StepsOf(variations[current])];
                reached = [.. reached.SelectMany(earlier => steps.Select(step => With(earlier, current, step)))];
                continue;
            }
            placed[partner] = true;
            int main = interlude.Main == sequence ? index : partner;
            int played = interlude.Main == sequence ? partner : index;
            JointStep[] joint = [.. JointSteps(variations[main].Pictures.Length, interlude.HeldStep, variations[played].Pictures.Length)];
            reached = [.. reached.SelectMany(earlier => joint.Select(step => With(With(earlier, main, step.Main), played, step.Interlude)))];
        }
        return reached;
    }

    /// <summary>
    /// The steps a main sequence of <paramref name="mainSteps"/> steps and an interlude of <paramref name="interludeSteps"/>
    /// steps it plays at <paramref name="heldStep"/> can have reached together: the main sequence up to the held step before
    /// the interlude starts, the held step through every step of the interlude, and every later main step once the
    /// interlude has played through. Its last step is the one it finishes on, whose picture any step it shows last gives.
    /// </summary>
    private static IEnumerable<JointStep> JointSteps(int mainSteps, int heldStep, int interludeSteps)
    {
        if (heldStep >= mainSteps)
            throw new InvalidDataException($"An interlude plays at step {heldStep} of a sequence of {mainSteps} steps.");
        yield return new JointStep(null, null);
        for (int step = 0; step <= heldStep; step++)
            yield return new JointStep(step, null);
        for (int played = 0; played < interludeSteps; played++)
            yield return new JointStep(heldStep, played);
        for (int step = heldStep + 1; step < mainSteps; step++)
            yield return new JointStep(step, interludeSteps - 1);
    }

    /// <summary>Every step of <paramref name="variation"/>'s sequence, after none.</summary>
    private static IEnumerable<int?> StepsOf(FrameArtworkVariation variation) =>
        Enumerable.Range(0, variation.Pictures.Length).Select(step => (int?)step).Prepend(null);

    /// <summary>The index of the variation following <paramref name="sequence"/>, or -1.</summary>
    private static int IndexOf(IReadOnlyList<FrameArtworkVariation> variations, EncounterArtworkSequence sequence)
    {
        for (int index = 0; index < variations.Count; index++)
        {
            if (variations[index].Sequence == sequence)
                return index;
        }
        return -1;
    }

    /// <summary>A copy of <paramref name="reached"/> with variation <paramref name="index"/> at <paramref name="step"/>.</summary>
    private static int?[] With(int?[] reached, int index, int? step)
    {
        int?[] copy = [.. reached];
        copy[index] = step;
        return copy;
    }

    /// <summary>The steps a main sequence and its interlude have reached together.</summary>
    /// <param name="Main">The main sequence's step, or none.</param>
    /// <param name="Interlude">The interlude's step, or none.</param>
    private readonly record struct JointStep(int? Main, int? Interlude);
}
