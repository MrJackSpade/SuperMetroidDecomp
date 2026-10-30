using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.BotwoonWallPlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The blocks and artwork name of Botwoon's wall.</summary>
internal static class BotwoonWallPlmDraws
{
    /// <summary>Nine identical air blocks; indexed only within the single vertical run. Editable appearance is applied
    /// separately.</summary>
    internal static RoomTerrainBlock BlockAt(int index)
    {
        if ((uint)index >= BlockCount) throw new ArgumentOutOfRangeException(nameof(index));
        return RoomTerrainBlocks.BlankAir;
    }

    /// <summary>Assigns the lone wall-clear draw its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(PlmDraw draw) => draw == Clear
        ? "clear-wall"
        : throw new InvalidDataException($"Botwoon wall draw {draw} has no visual ID.");

    /// <summary>Materializes the nine-block draw record for asset tooling; gameplay evaluates the compiled fill directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var blocks = new RoomTerrainBlock[BlockCount];
            for (int index = 0; index < blocks.Length; index++) blocks[index] = BlockAt(index);
            yield return new(Clear, new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(PlmRunDirection.Vertical, blocks, new(0, 0)),
            });
        }
    }
}
