using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// Presentation-only elevator-platform block selection. The four-step
/// instruction loop, draw geometry, and terrain blocks stay compiled.
/// </summary>
public sealed class RoomObjectElevatorPlatformVisualCatalog
{
    /// <summary>Only frames whose appearances differ from the compiled source are stored here.</summary>
    private readonly IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[][]> customAppearances;

    /// <summary>Holds the import-validated run appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customAppearances">The changed frames' appearances run by run, keyed by draw.</param>
    internal RoomObjectElevatorPlatformVisualCatalog(IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[][]> customAppearances) =>
        this.customAppearances = Ensure.NotNull(customAppearances);

    /// <summary>Resolves one elevator-platform block appearance without replacing its compiled terrain block or draw offset.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the draw run.</param>
    /// <param name="blockIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected block and mirroring, taken from the compiled stock frame when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        ElevatorPlatformArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
