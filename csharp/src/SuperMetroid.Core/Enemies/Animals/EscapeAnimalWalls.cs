using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Lays the escape animals' rescue walls.</summary>
internal static class EscapeAnimalWalls
{
    /// <summary>Maps one of four rescue-wall draw identities to three vertical solid blocks, or rejects another family's draw.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (request is not PlmFrameDraw<EscapeAnimalDraw> { Frame: var list })
        {
            draw = default;
            return false;
        }
        RoomLevelWord word = EscapeAnimalPlmDrawDefinitions.Cells.TryGetValue(list, out RoomLevelWord cell)
            ? cell
            : throw new InvalidOperationException($"Undefined EscapeAnimalDraw {list}.");
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(PlmRunDirection.Vertical, Enumerable.Repeat(word.Raw, EscapeAnimalPlmDrawDefinitions.WallHeight).ToArray(), new(0, 0)),
        });
        return true;
    }
}
