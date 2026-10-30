using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Visual-only Mother Brain fake-death wall, room background, door, and tube
/// appearances. Collision kinds, draw geometry, and PLM timing remain
/// immutable compiled data, independent of these replacement tile indexes.
/// </summary>
public sealed class RoomPlmMotherBrainFakeDeathVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => MotherBrainFakeDeathArtwork.ContentIdentity(this);

    /// <summary>Only changed draw appearances, keyed by draw; unchanged draws use compiled visuals.</summary>
    private readonly PlmVisualOverrides customAppearances;

    /// <summary>Validates all twenty-two fake-death room layouts and copies visual differences while retaining collision kinds, signed draw offsets, and PLM timing.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, with the exact flattened count of all its native run cells.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmMotherBrainFakeDeathVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customAppearances = new PlmVisualOverrides(entries, "Mother Brain fake-death", MotherBrainFakeDeathDraws.TryGetByVisualId,
            MotherBrainFakeDeathDraws.All.Count());

    /// <summary>Resolves a fake-death room block's selected appearance, projecting native run-local indices into its flattened authored layout.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based native run ordinal; background rows have one thirteen-block horizontal run, while other layouts retain their own run shapes.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within the run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored block and mirroring, or the compiled terrain block's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported fake-death layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        MotherBrainFakeDeathArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
