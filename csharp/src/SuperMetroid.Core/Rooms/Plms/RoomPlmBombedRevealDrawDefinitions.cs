using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Non-breaking reveals of gated shootable parents in bank $84.</summary>
internal static class RoomPlmBombedRevealDrawDefinitions
{
    /// <summary>The single level word each bombed reveal draws.</summary>
    internal static readonly IReadOnlyDictionary<RoomPlmBombedRevealDraw, RoomLevelWord> Words = new Dictionary<RoomPlmBombedRevealDraw, RoomLevelWord>
    {
        [RoomPlmBombedRevealDraw.PowerBomb] = RoomLevelWord.Create(0x057, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        [RoomPlmBombedRevealDraw.SuperMissile] = RoomLevelWord.Create(0x09f, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        [RoomPlmBombedRevealDraw.CrumbleSingle] = RoomLevelWord.Create(0x0bc, BlockMirroring.None, RoomCollisionType.SpecialBlock),
    };
}
