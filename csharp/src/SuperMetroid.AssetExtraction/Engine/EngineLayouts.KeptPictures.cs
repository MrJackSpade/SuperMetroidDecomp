using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Changes to a room's blocks that change what they are and not how they look.</summary>
internal sealed partial class EngineLayouts
{
    /// <summary>
    /// The blocks of a patch that gives the blocks of a shape at one place their kinds and leaves each showing what
    /// the level has it show: a wall nobody sees. Each lies from the level's first block, so a patch of several such
    /// shapes has its origin there.
    /// </summary>
    /// <exception cref="InvalidDataException">The layout has not been built, a block lies outside the level, or one shows something a patch cannot.</exception>
    internal IReadOnlyList<PatchCell> CellsKeepingPictures(RoomDefinition room, RoomStateSettings state, Position16<RoomBlocks> at, RoomBlockShape shape)
    {
        string path = PathOf(room, state);
        Layout layout = _layouts.TryGetValue(path, out Layout? built)
            ? built
            : throw new InvalidDataException($"The layout {path} has not been built, and is asked what its blocks show.");
        var cells = new List<PatchCell>();
        foreach (RoomBlockShapeCell cell in shape.Cells)
        {
            int x = at.X + cell.Cell.X, y = at.Y + cell.Cell.Y;
            if (x < 0 || y < 0 || x >= layout.Level.Size.X || y >= layout.Level.Size.Y)
                throw new InvalidDataException($"A change to the layout {path} reaches the block {x} across and {y} down, which is outside its level.");
            RoomBlockAppearance shown = layout.Level.Foreground[y * layout.Level.Size.X + x].Appearance;
            cells.Add(shown.Shows switch
            {
                RoomBlockShown.Block => new PatchCell(x, y, cell.Collision, shown.Block, shown.Mirroring),
                RoomBlockShown.Nothing => new PatchCell(x, y, cell.Collision, null, BlockMirroring.None),
                _ => throw new InvalidDataException($"The block {x} across and {y} down of the layout {path} shows {shown}, which a patch cannot keep."),
            });
        }

        return cells;
    }
}
