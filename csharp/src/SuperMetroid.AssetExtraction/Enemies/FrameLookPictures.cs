using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The finished pictures a sheet holds of a frame encounters' looks change (#1538): one for each combination of the looks
/// the encounters show while it is drawn, one look of each. Encounters play independently of one another.
/// </summary>
internal static class FrameLookPictures
{
    /// <summary>
    /// Each combination of the looks <paramref name="changes"/> show while the frame is drawn, one look of each encounter in
    /// the changes' order.
    /// </summary>
    internal static IEnumerable<IReadOnlyList<EncounterLook>> Combinations(IReadOnlyList<FrameLookChange> changes)
    {
        Ensure.NotNull(changes);
        IEnumerable<IReadOnlyList<EncounterLook>> combinations = [[]];
        foreach (FrameLookChange change in changes)
            combinations = [.. combinations.SelectMany(earlier => change.Looks.Select(look =>
                (IReadOnlyList<EncounterLook>)[.. earlier, new EncounterLook(change.Encounter, look)]))];
        return combinations;
    }

    /// <summary>The names of the pictures a sheet holds of <paramref name="frame"/>, which <paramref name="changes"/> change.</summary>
    internal static IEnumerable<string> Names(string frame, IReadOnlyList<FrameLookChange> changes) =>
        Combinations(changes).Select(combination => EncounterLook.PictureName(frame, combination));
}
