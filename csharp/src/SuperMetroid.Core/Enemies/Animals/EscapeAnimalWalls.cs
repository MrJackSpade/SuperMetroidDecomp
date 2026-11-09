using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Lays the escape animals' rescue walls.</summary>
internal static class EscapeAnimalWalls
{
    /// <summary>Maps one of four rescue-wall draw identities to three vertical solid cells, or rejects another family's draw.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        if (request is not RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: var list })
        {
            draw = default;
            return false;
        }
        RoomCollisionType collision = EscapeAnimalObjectDrawDefinitions.Collisions.TryGetValue(list, out RoomCollisionType cell)
            ? cell
            : throw new InvalidOperationException($"Undefined EscapeAnimalDraw {list}.");
        draw = new(request, new RoomObjectShotBlockDrawDefinitions.Run[]
        {
            new(RoomObjectRunDirection.Vertical, Enumerable.Repeat(collision, EscapeAnimalObjectDrawDefinitions.WallHeight).ToArray(), new(0, 0)),
        });
        return true;
    }
}
