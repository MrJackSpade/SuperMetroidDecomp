using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's ceiling block draws: which PLM draws are the ceiling's and the cell each fills its two-by-two square
/// with from <see cref="SporeSpawnCeilingPlmDrawDefinitions"/>.
/// </summary>
internal static class SporeSpawnCeilingDraws
{
    /// <summary>The one air cell a ceiling draw fills its square with.</summary>
    internal static bool TryGetBlock(PlmDraw request, out RoomTerrainBlock block)
    {
        if (request is not PlmFrameDraw<SporeSpawnCeilingDraw>)
        {
            block = default;
            return false;
        }
        var list = PlmDraw.FrameOf<SporeSpawnCeilingDraw>(request, "SporeSpawnCeilingDraw");
        block = SporeSpawnCeilingPlmDrawDefinitions.Cells.TryGetValue(list, out RoomTerrainBlock cell)
            ? cell
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {list}.");
        return true;
    }
}
