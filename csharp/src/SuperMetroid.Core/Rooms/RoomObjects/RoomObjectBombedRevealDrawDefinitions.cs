using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Non-breaking reveals of gated shootable parents; what each shows is its installed picture (#1557).</summary>
internal static class RoomObjectBombedRevealDrawDefinitions
{
    /// <summary>How the single block each bombed reveal draws collides.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectBombedRevealDraw, RoomCollisionType> Collisions = new Dictionary<RoomObjectBombedRevealDraw, RoomCollisionType>
    {
        [RoomObjectBombedRevealDraw.PowerBomb] = RoomCollisionType.ShootableBlock,
        [RoomObjectBombedRevealDraw.SuperMissile] = RoomCollisionType.ShootableBlock,
        [RoomObjectBombedRevealDraw.CrumbleSingle] = RoomCollisionType.SpecialBlock,
    };
}
