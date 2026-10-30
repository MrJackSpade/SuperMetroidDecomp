using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Physical four-block draw lists for grey door caps and the four shared clear-cap
/// frames. Bomb Torizo's special resident door uses the ordinary right-facing art.
/// </summary>
internal static class GreyDoorPlmDrawDefinitions
{
    /// <summary>The cleared-opening frame, which draws no cap.</summary>
    internal const int ClearFrame = -1;

    /// <summary>
    /// The four cells of each of the twenty grey-cap draws at $84:A677..A766, in native run order. The clear frame
    /// has no collision; frame zero carries the shootable and extension types; opening frames are solid except the
    /// middle cells of final left and down frames, which are air. Vertical tiles advance one per frame with a 32-tile
    /// inner-row stride; horizontal tiles alternate tileset rows, advancing two every two frames. Mirrors follow the
    /// facing and the cell's half of the cap.
    /// </summary>
    internal static IReadOnlyDictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomLevelWord>> Cells { get; } =
        new Dictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomLevelWord>>
        {
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, ClearFrame)] =
            [
                RoomLevelWord.Create(0x82, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x82, BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0xae, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xce, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xce, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xae, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0xaf, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xaf, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0xb0, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb0, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0xb1, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd1, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xd1, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0xb1, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, ClearFrame)] =
            [
                RoomLevelWord.Create(0x82, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x82, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0xae, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xce, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xce, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xae, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0xaf, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xaf, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0xb0, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb0, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0xb1, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd1, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd1, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb1, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, ClearFrame)] =
            [
                RoomLevelWord.Create(0x84, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x84, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0xb3, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xb2, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb2, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb3, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0xd3, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd3, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0xb5, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb5, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0xd5, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd4, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd4, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd5, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, ClearFrame)] =
            [
                RoomLevelWord.Create(0x84, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x84, BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0xb3, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xb2, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb2, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb3, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0xd3, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd3, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0xb5, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb5, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0xd5, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd4, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0xd4, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0xd5, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
