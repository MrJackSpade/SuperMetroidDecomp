using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Non-breaking reveals of gated shootable parents.</summary>
internal static class RoomObjectBombedRevealDrawDefinitions
{
    /// <summary>The single block each bombed reveal draws.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectBombedRevealDraw, RoomTerrainBlock> Blocks = new Dictionary<RoomObjectBombedRevealDraw, RoomTerrainBlock>
    {
        [RoomObjectBombedRevealDraw.PowerBomb] = RoomTerrainBlock.Of(87, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        [RoomObjectBombedRevealDraw.SuperMissile] = RoomTerrainBlock.Of(159, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        [RoomObjectBombedRevealDraw.CrumbleSingle] = RoomTerrainBlock.Of(188, BlockMirroring.None, RoomCollisionType.SpecialBlock),
    };
}
