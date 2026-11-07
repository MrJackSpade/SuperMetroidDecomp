using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Visual-only Mother Brain fake-death wall, room background, door, and tube
/// appearances. Collision kinds, draw geometry, and PLM timing remain
/// immutable compiled data, independent of these replacement tile indexes.
/// </summary>
public sealed class RoomObjectMotherBrainFakeDeathVisualCatalog
{
    /// <summary>Only changed draw appearances, keyed by draw; unchanged draws use compiled visuals.</summary>
    private readonly RoomObjectVisualOverrides customAppearances;

    /// <summary>Holds the import-validated appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customAppearances">The changed layouts' flattened appearances, keyed by draw.</param>
    internal RoomObjectMotherBrainFakeDeathVisualCatalog(RoomObjectVisualOverrides customAppearances) =>
        this.customAppearances = Ensure.NotNull(customAppearances);

    /// <summary>Resolves a fake-death room block's selected appearance, projecting run-local indices into its flattened authored layout.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based run ordinal; background rows have one thirteen-block horizontal run, while other layouts retain their own run shapes.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within the run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored block and mirroring, or the compiled terrain block's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported fake-death layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        MotherBrainFakeDeathArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
