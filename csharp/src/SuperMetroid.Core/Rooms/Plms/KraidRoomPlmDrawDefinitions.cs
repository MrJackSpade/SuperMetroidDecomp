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
        [KraidRoomDraw.CrumbleFirst] = RoomTerrainBlock.Of(384, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [KraidRoomDraw.CrumbleSecond] = RoomTerrainBlock.Of(385, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [KraidRoomDraw.CrumbleThird] = RoomTerrainBlock.Of(386, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground1] = RoomTerrainBlock.Of(316, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground2] = RoomTerrainBlock.Of(305, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground3] = RoomTerrainBlock.Of(304, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.SpikeFirst] = RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.SpikeSecond] = RoomTerrainBlock.Of(272, BlockMirroring.None, RoomCollisionType.Air),
    };

    /// <summary>Number of reachable Kraid ceiling and spike draw lists.</summary>
    internal const int DrawCount = 10;
}
