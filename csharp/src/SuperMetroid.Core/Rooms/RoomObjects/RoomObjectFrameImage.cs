using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of one room-object frame (#1557): at each cell the frame covers, by the cell's place from the
/// object's origin, the block of the room's block sheet it shows and how that block is mirrored. The import pictures
/// every frame; the game shows it as installed. How the frame's cells collide stays with the frame's own geometry.
/// </summary>
public sealed class RoomObjectFrameImage
{
    /// <summary>What the frame shows at each cell it covers, by the cell's place from the object's origin.</summary>
    private readonly IReadOnlyDictionary<Offset16<RoomBlocks>, RoomBlockAppearance> cells;

    /// <summary>Holds a frame's picture, which the import has checked covers exactly the frame's cells.</summary>
    /// <param name="cells">What the frame shows at each cell, owned by the picture.</param>
    internal RoomObjectFrameImage(IReadOnlyDictionary<Offset16<RoomBlocks>, RoomBlockAppearance> cells) =>
        this.cells = Ensure.NotNull(cells);

    /// <summary>Every cell the frame shows, by its place from the object's origin.</summary>
    internal IReadOnlyDictionary<Offset16<RoomBlocks>, RoomBlockAppearance> Cells => cells;

    /// <summary>What the frame shows at <paramref name="cell"/>, a place from the object's origin.</summary>
    /// <exception cref="InvalidDataException">The frame's picture covers no cell there.</exception>
    internal RoomBlockAppearance At(Offset16<RoomBlocks> cell) =>
        cells.TryGetValue(cell, out RoomBlockAppearance shown)
            ? shown
            : throw new InvalidDataException($"The installed room-object frame covers no cell at ({cell.X},{cell.Y}).");
}
