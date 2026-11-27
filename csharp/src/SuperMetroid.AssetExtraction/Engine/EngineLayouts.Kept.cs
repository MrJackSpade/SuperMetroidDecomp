using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What a patch needs to know of the level it changes: a block as the level has it.</summary>
internal sealed partial class EngineLayouts
{
    /// <summary>
    /// One block of a patch that keeps the picture the level gives the block there and becomes another kind of block:
    /// what the old game does where it changes a block's kind and nothing else.
    /// </summary>
    /// <param name="state">The state of a room whose level it is.</param>
    /// <param name="origin">The patch's origin.</param>
    /// <param name="across">How many blocks right of the origin the block is.</param>
    /// <param name="down">How many blocks below it.</param>
    /// <param name="collision">The kind of block it becomes.</param>
    /// <exception cref="InvalidDataException">The block lies outside the level.</exception>
    internal PatchCell Kept(RoomStateSettings state, Position16<RoomBlocks> origin, int across, int down, RoomCollisionType collision)
    {
        InstalledRoomLevelBlock block = BlockAt(state, origin.X + across, origin.Y + down);
        RoomBlockAppearance shown = block.Terrain.Appearance;
        return shown.Shows == RoomBlockShown.Block
            ? new PatchCell(across, down, collision, shown.Block, shown.Mirroring)
            : new PatchCell(across, down, collision, null, BlockMirroring.None);
    }

    /// <summary>
    /// Gives the layout of one state of a room a named patch that is one reacting block, a tile wide and so many
    /// tall, following rules given: what the old game makes where it gives blocks a reaction the level does not have.
    /// Each tile keeps the picture the level gives it.
    /// </summary>
    /// <param name="blockName">The name of the block's rules, as its data file is named.</param>
    /// <param name="rules">The rules.</param>
    /// <exception cref="InvalidDataException">The layout is not built, the patch lies outside the level, or it is given twice with different blocks.</exception>
    internal void PatchReactingBlock(RoomDefinition room, RoomStateSettings state, string name, Position16<RoomBlocks> origin, int height, string blockName, JsonObject rules)
    {
        string path = PathOf(room, state);
        Layout layout = _layouts.TryGetValue(path, out Layout? built)
            ? built
            : throw new InvalidDataException($"The layout {path} has not been built, and is given the patch {name}.");
        var rows = new JsonArray();
        for (int row = 0; row < height; row++)
        {
            RoomBlockAppearance shown = BlockAt(state, origin.X, origin.Y + row).Terrain.Appearance;
            // The block's own tile is its topmost; those below it are the block's too, and only show themselves.
            var tile = new JsonObject { ["kind"] = row == 0 ? "block" : "empty" };
            if (row == 0)
            {
                tile["block"] = BlockType(new ReactingBlock(blockName, rules, 1, height));
                if (height > 1)
                    tile["height"] = height;
            }

            if (shown.Shows == RoomBlockShown.Block && Picture(layout.Tileset, shown.Block, shown.Mirroring) is { } picture)
                tile["picture"] = picture;
            rows.Add(Letter(layout, tile, path).ToString());
        }

        var patch = new JsonObject { ["column"] = (int)origin.X, ["row"] = (int)origin.Y, ["rows"] = rows };
        if (layout.Patches[name] is { } given)
        {
            if (given.ToJsonString() != patch.ToJsonString())
                throw new InvalidDataException($"The layout {path} is given the patch {name} twice, with different blocks.");
            return;
        }

        layout.Patches[name] = patch;
    }

    /// <summary>
    /// The pictures of a column of tiles, top to bottom, as one of the old game's room object frames draws them: each
    /// named by the room's set of graphics, so that one block type serves every set, and written for the set of the
    /// state of a room that shows them. Empty for a tile the frame draws nothing at.
    /// </summary>
    /// <exception cref="InvalidDataException">The state's set of graphics draws nothing for a block the frame shows.</exception>
    internal JsonArray ColumnPictures(RoomStateSettings state, string directory, string frame, int height)
    {
        string tileset = state.GraphicsSet.ToString();
        var pictures = new JsonArray();
        for (int row = 0; row < height; row++)
        {
            if (FrameBlock(directory, frame, 0, row) is not { } drawn)
            {
                pictures.Add("");
                continue;
            }

            if (Picture(tileset, drawn.Block, drawn.Mirroring) is null)
                throw new InvalidDataException($"The graphics {tileset} draw nothing for the block {drawn.Block}, which the frame {frame} shows.");
            string across = drawn.Mirroring.HasFlag(BlockMirroring.Horizontal) ? "_h" : "";
            string down = drawn.Mirroring.HasFlag(BlockMirroring.Vertical) ? "_v" : "";
            pictures.Add($"tiles/{{graphics}}/{drawn.Block}{across}{down}.png");
        }

        return pictures;
    }

    /// <summary>A block of a level: what it is and where it lies.</summary>
    internal readonly partial record struct InstalledRoomLevelBlock(Position16<RoomBlocks> At, RoomTerrainBlock Terrain);

    /// <summary>Every block of the level of one state of a room, row by row.</summary>
    internal IEnumerable<InstalledRoomLevelBlock> Blocks(RoomStateSettings state)
    {
        var level = _levels.Get(state.Level);
        for (int y = 0; y < level.Size.Y; y++)
        {
            for (int x = 0; x < level.Size.X; x++)
                yield return new InstalledRoomLevelBlock(new Position16<RoomBlocks>(x, y), level.Foreground[y * level.Size.X + x]);
        }
    }

    /// <summary>The block of a level at a place.</summary>
    /// <exception cref="InvalidDataException">The place lies outside the level.</exception>
    internal InstalledRoomLevelBlock BlockAt(RoomStateSettings state, int x, int y)
    {
        var level = _levels.Get(state.Level);
        if (x < 0 || y < 0 || x >= level.Size.X || y >= level.Size.Y)
            throw new InvalidDataException($"The level {state.Level.Name} has no block {x} across and {y} down.");
        return new InstalledRoomLevelBlock(new Position16<RoomBlocks>(x, y), level.Foreground[y * level.Size.X + x]);
    }
}
