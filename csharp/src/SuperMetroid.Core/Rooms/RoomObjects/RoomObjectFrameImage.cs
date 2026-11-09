using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of one room-object frame (#1557): at each cell the frame covers, by the cell's place from the
/// object's origin, the block of the room's block sheet it shows and how that block is mirrored. The import pictures
/// every frame; the game shows it as installed. How the frame's cells collide stays with the frame's own geometry.
/// </summary>
public sealed class RoomObjectFrameImage
{

    /// <summary>Holds a frame's picture, which the import has checked covers exactly the frame's cells.</summary>
    /// <param name="cells">What the frame shows at each cell, owned by the picture.</param>
    internal RoomObjectFrameImage(IReadOnlyDictionary<Offset16<RoomBlocks>, RoomBlockAppearance> cells) =>
        Cells = Ensure.NotNull(cells);

    /// <summary>Every cell the frame shows, by its place from the object's origin.</summary>
    internal IReadOnlyDictionary<Offset16<RoomBlocks>, RoomBlockAppearance> Cells { get; }

    /// <summary>What the frame shows at <paramref name="cell"/>, a place from the object's origin.</summary>
    /// <exception cref="InvalidDataException">The frame's picture covers no cell there.</exception>
    internal RoomBlockAppearance At(Offset16<RoomBlocks> cell) =>
        Cells.TryGetValue(cell, out RoomBlockAppearance shown)
            ? shown
            : throw new InvalidDataException($"The installed room-object frame covers no cell at ({cell.X},{cell.Y}).");

    /// <summary>
    /// This picture with every cell's block flipped left to right, as a mirrored twin of a frame shows the frame it
    /// mirrors; the cells keep their places.
    /// </summary>
    internal RoomObjectFrameImage MirroredHorizontally() =>
        new(Cells.ToDictionary(cell => cell.Key, cell => cell.Value.WithMirroring(cell.Value.Mirroring ^ BlockMirroring.Horizontal)));
}
