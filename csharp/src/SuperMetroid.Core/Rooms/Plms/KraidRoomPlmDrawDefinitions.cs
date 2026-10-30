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
    internal static readonly IReadOnlyDictionary<KraidRoomDraw, RoomLevelWord> FixedWords = new Dictionary<KraidRoomDraw, RoomLevelWord>
    {
        [KraidRoomDraw.CrumbleFirst] = RoomLevelWord.Create(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [KraidRoomDraw.CrumbleSecond] = RoomLevelWord.Create(0x181, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [KraidRoomDraw.CrumbleThird] = RoomLevelWord.Create(0x182, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground1] = RoomLevelWord.Create(0x13c, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground2] = RoomLevelWord.Create(0x131, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.CeilingBackground3] = RoomLevelWord.Create(0x130, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.SpikeFirst] = RoomLevelWord.Create(0x111, BlockMirroring.None, RoomCollisionType.Air),
        [KraidRoomDraw.SpikeSecond] = RoomLevelWord.Create(0x110, BlockMirroring.None, RoomCollisionType.Air),
    };

    /// <summary>Number of reachable Kraid ceiling and spike draw lists.</summary>
    internal const int DrawCount = 10;
}
