using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Complete, immutable presentation selection for the 19 ordinary shot-block lists.</summary>
public sealed class InstalledRoomPlmShotBlockVisuals
{
    /// <summary>Deep-copied edited run appearances keyed by draw; stock-identical draw lists stay calculated.</summary>
    private readonly IReadOnlyDictionary<PlmDraw, RoomBlockAppearance[][]> customBlocks;

    /// <summary>Holds the import-validated run appearances of the draw lists that differ from the calculated draws.</summary>
    /// <param name="customBlocks">The changed draw lists' appearances run by run, keyed by draw.</param>
    internal InstalledRoomPlmShotBlockVisuals(IReadOnlyDictionary<PlmDraw, RoomBlockAppearance[][]> customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Returns one visual reference without exposing the catalog's mutable backing arrays.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (draw is not PlmFrameDraw<ShotBlockPlmDraws.Draw> { Frame: var shape })
            throw new InvalidDataException($"Shot-block visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlocksPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGetValue(draw, out var runs)
            ? runs[runIndex][blockIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
