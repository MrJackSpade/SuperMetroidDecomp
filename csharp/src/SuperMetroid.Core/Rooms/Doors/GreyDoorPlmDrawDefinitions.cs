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
                RoomLevelWord.Create(0x82, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x82, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0xae, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xce, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xce, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xae, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0xaf, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xaf, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0xb0, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb0, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0xb1, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd1, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xd1, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0xb1, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, ClearFrame)] =
            [
                RoomLevelWord.Create(0x82, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0xa2, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x82, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0xae, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xce, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xce, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0xae, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0xaf, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xcf, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xaf, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0xb0, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd0, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb0, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0xb1, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd1, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd1, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb1, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, ClearFrame)] =
            [
                RoomLevelWord.Create(0x84, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x84, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0xb3, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xb2, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb2, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb3, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0xd3, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd3, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0xb5, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb5, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0xd5, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd4, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd4, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd5, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, ClearFrame)] =
            [
                RoomLevelWord.Create(0x84, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x83, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x84, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0xb3, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xb2, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb2, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0xb3, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0xd3, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd2, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd3, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0xb5, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb4, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xb5, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0xd5, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xd4, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0xd4, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0xd5, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
