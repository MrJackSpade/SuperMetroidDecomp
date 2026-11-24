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
