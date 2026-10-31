using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Non-breaking reveals of gated shootable parents.</summary>
internal static class RoomPlmBombedRevealDrawDefinitions
{
    /// <summary>The single block each bombed reveal draws.</summary>
    internal static readonly IReadOnlyDictionary<RoomPlmBombedRevealDraw, RoomTerrainBlock> Blocks = new Dictionary<RoomPlmBombedRevealDraw, RoomTerrainBlock>
    {
        [RoomPlmBombedRevealDraw.PowerBomb] = RoomTerrainBlock.Of(87, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        [RoomPlmBombedRevealDraw.SuperMissile] = RoomTerrainBlock.Of(159, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        [RoomPlmBombedRevealDraw.CrumbleSingle] = RoomTerrainBlock.Of(188, BlockMirroring.None, RoomCollisionType.SpecialBlock),
    };
}
