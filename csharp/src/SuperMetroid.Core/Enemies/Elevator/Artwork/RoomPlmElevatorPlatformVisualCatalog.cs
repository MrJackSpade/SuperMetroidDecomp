using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// Presentation-only elevator-platform block selection. The native four-step
/// instruction loop, draw geometry, and terrain blocks stay compiled.
/// </summary>
public sealed class RoomPlmElevatorPlatformVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => ElevatorPlatformArtwork.ContentIdentity(this);

    /// <summary>Only frames whose appearances differ from the compiled source are stored here.</summary>
    private readonly Dictionary<PlmDraw, RoomBlockAppearance[][]>? customAppearances;

    /// <summary>Validates all three elevator-platform draw layouts and copies visual differences while retaining native run geometry, terrain blocks, and animation cadence.</summary>
    /// <remarks>The four-step instruction loop selects first, second, third, then second layout; the repeated second layout has only one entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled frame, with its exact native run and cell counts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmElevatorPlatformVisualCatalog(
        IEnumerable<RoomPlmVisualRunEntry> entries) =>
        customAppearances = ElevatorPlatformArtwork.Overrides(entries);

    /// <summary>Resolves one elevator-platform block appearance without replacing its compiled terrain block or draw offset.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the native draw run.</param>
    /// <param name="blockIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected block and mirroring, taken from the compiled stock frame when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        ElevatorPlatformArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
