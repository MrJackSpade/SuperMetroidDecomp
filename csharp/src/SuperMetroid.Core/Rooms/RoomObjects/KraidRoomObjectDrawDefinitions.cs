using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Reachable Kraid ceiling and spike draws. The first is also used by the Maridia elevatube, but its collision is the
/// same for either PLM owner. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class KraidRoomObjectDrawDefinitions
{
    /// <summary>How each named crumble, background and spike block collides.</summary>
    internal static readonly IReadOnlyDictionary<KraidRoomDraw, RoomCollisionType> FixedCollisions = new Dictionary<KraidRoomDraw, RoomCollisionType>
    {
        [KraidRoomDraw.CrumbleFirst] = RoomCollisionType.SolidBlock,
        [KraidRoomDraw.CrumbleSecond] = RoomCollisionType.SolidBlock,
        [KraidRoomDraw.CrumbleThird] = RoomCollisionType.Air,
        [KraidRoomDraw.CeilingBackground1] = RoomCollisionType.Air,
        [KraidRoomDraw.CeilingBackground2] = RoomCollisionType.Air,
        [KraidRoomDraw.CeilingBackground3] = RoomCollisionType.Air,
        [KraidRoomDraw.SpikeFirst] = RoomCollisionType.Air,
        [KraidRoomDraw.SpikeSecond] = RoomCollisionType.Air,
    };
}
