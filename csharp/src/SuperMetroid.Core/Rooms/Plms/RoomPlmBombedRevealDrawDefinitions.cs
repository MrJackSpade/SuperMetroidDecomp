using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Non-breaking reveals of gated shootable parents in bank $84.</summary>
internal static class RoomPlmBombedRevealDrawDefinitions
{
    /// <summary>The single level word each bombed reveal draws.</summary>
    internal static readonly IReadOnlyDictionary<RoomPlmBombedRevealDraw, ushort> Words = new Dictionary<RoomPlmBombedRevealDraw, ushort>
    {
        [RoomPlmBombedRevealDraw.PowerBomb] = RoomLevelWord.Create(0x057, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock).Raw,
        [RoomPlmBombedRevealDraw.SuperMissile] = RoomLevelWord.Create(0x09f, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock).Raw,
        [RoomPlmBombedRevealDraw.CrumbleSingle] = RoomLevelWord.Create(0x0bc, LevelBlockFlipFlags.None, RoomCollisionType.SpecialBlock).Raw,
    };
}
