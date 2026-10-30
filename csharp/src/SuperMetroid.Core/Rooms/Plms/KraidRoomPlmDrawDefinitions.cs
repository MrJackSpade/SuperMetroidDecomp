using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Reachable Kraid ceiling and spike draws. The first is also used by the Maridia elevatube, but its terrain is the
/// same for either PLM owner.
/// </summary>
internal static class KraidRoomPlmDrawDefinitions
{
    /// <summary>The terrain of each named crumble and background block, with its collision kind.</summary>
    internal static readonly IReadOnlyDictionary<KraidRoomDraw, RoomTerrainBlock> FixedBlocks = new Dictionary<KraidRoomDraw, RoomTerrainBlock>
    {
        [KraidRoomDraw.CrumbleFirst] = RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [KraidRoomDraw.CrumbleSecond] = RoomTerrainBlock.Of(0x181, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [KraidRoomDraw.CrumbleThird] = RoomTerrainBlock.Of(0x182, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground1] = RoomTerrainBlock.Of(0x13c, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground2] = RoomTerrainBlock.Of(0x131, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground3] = RoomTerrainBlock.Of(0x130, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.SpikeFirst] = RoomTerrainBlock.Of(0x111, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.SpikeSecond] = RoomTerrainBlock.Of(0x110, BlockMirroring.None, RoomCollisionType.Air),
    };

    /// <summary>Number of reachable Kraid ceiling and spike draw lists.</summary>
    internal const int DrawCount = 10;
}
