using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Reachable Kraid ceiling and spike draw lists at $84:9367..93EE. The
/// first list is also used by the Maridia elevatube, but its physical word
/// is the same for either PLM owner.
/// </summary>
internal static class KraidRoomPlmDrawDefinitions
{
    /// <summary>The level word of each named crumble and background block; full words keep their collision nibbles.</summary>
    internal static readonly IReadOnlyDictionary<KraidRoomDraw, ushort> FixedWords = new Dictionary<KraidRoomDraw, ushort>
    {
        [KraidRoomDraw.CrumbleFirst] = RoomLevelWord.Create(0x180, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock).Raw,
        [KraidRoomDraw.CrumbleSecond] = RoomLevelWord.Create(0x181, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock).Raw,
        [KraidRoomDraw.CrumbleThird] = RoomLevelWord.Create(0x182, LevelBlockFlipFlags.None, RoomCollisionType.Air).Raw,
        [KraidRoomDraw.CeilingBackground1] = RoomLevelWord.Create(0x13c, LevelBlockFlipFlags.None, RoomCollisionType.Air).Raw,
        [KraidRoomDraw.CeilingBackground2] = RoomLevelWord.Create(0x131, LevelBlockFlipFlags.None, RoomCollisionType.Air).Raw,
        [KraidRoomDraw.CeilingBackground3] = RoomLevelWord.Create(0x130, LevelBlockFlipFlags.None, RoomCollisionType.Air).Raw,
        [KraidRoomDraw.SpikeFirst] = RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.Air).Raw,
        [KraidRoomDraw.SpikeSecond] = RoomLevelWord.Create(0x110, LevelBlockFlipFlags.None, RoomCollisionType.Air).Raw,
    };

    /// <summary>Number of reachable Kraid ceiling and spike draw lists.</summary>
    internal const int DrawCount = 10;
}
