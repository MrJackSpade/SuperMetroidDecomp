using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The three physical four-block gate images chosen by the Mother Brain
/// escape-gate program. Level words remain immutable collision definitions;
/// visual block references can be separated for authoring independently.
/// </summary>
internal static class MotherBrainEscapeGatePlmDrawDefinitions
{
    /// <summary>
    /// $84:9473..9496: each gate image's four vertical cells, always solid. Open uses blank FF; half adds end caps
    /// 30F; closed fills the middle with tile 2E8, vertically flipped for the upper half. Collision nibble 8 is
    /// unchanged even for the visually open frame.
    /// </summary>
    internal static readonly Dictionary<MotherBrainEscapeGateDraw, RoomLevelWord[]> Cells = new()
    {
        [MotherBrainEscapeGateDraw.Open] =
        [
            RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
        ],
        [MotherBrainEscapeGateDraw.HalfClosed] =
        [
            RoomLevelWord.Create(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock),
        ],
        [MotherBrainEscapeGateDraw.Closed] =
        [
            RoomLevelWord.Create(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0x2e8, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0x2e8, BlockMirroring.None, RoomCollisionType.SolidBlock),
            RoomLevelWord.Create(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock),
        ],
    };

    /// <summary>Each gate image's stable authored visual identifier.</summary>
    internal static readonly Dictionary<MotherBrainEscapeGateDraw, string> VisualIds = new()
    {
        [MotherBrainEscapeGateDraw.Open] = "open",
        [MotherBrainEscapeGateDraw.HalfClosed] = "half-closed",
        [MotherBrainEscapeGateDraw.Closed] = "closed",
    };
}
