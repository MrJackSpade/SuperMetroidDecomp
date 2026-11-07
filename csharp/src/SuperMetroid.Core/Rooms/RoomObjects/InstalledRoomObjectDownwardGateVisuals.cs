using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Presentation-only selection for downward-gate PLM blocks. Physical level appearances,
/// gate collision, shot filters, and animation timing remain compiled.
/// </summary>
public sealed class InstalledRoomObjectDownwardGateVisuals
{
    /// <summary>
    /// Deep-copied visual overrides keyed by draw; a gate frame absent here still takes its appearance from the compiled
    /// physical appearances.
    /// </summary>
    private readonly IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[][]> customBlocks;

    /// <summary>Holds the import-validated run appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customBlocks">The changed layouts' appearances run by run, keyed by draw.</param>
    internal InstalledRoomObjectDownwardGateVisuals(IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[][]> customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one gate or shot-trigger appearance using the compiled layout's run structure.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the draw run.</param>
    /// <param name="blockIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected appearance, or the compiled terrain's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported gate or shot-trigger layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!DownwardGateObjectDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException($"Downward gate visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGetValue(draw, out var runs)
            ? runs[runIndex][blockIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
