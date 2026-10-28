using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Yellow, green and red door-cap physical cells.</summary>
internal static class ColoredDoorPlmDrawDefinitions
{
    /// <summary>
    /// The four cells of each colour's cap frames, in native run order. Tiles advance four per colour, one per vertical
    /// frame, or alternate tileset rows for horizontal frames; mirrors follow the facing and the cell's half of the cap.
    /// Closed caps keep the shootable and extension collision types. Final middle cells clear for every left cap,
    /// green and red right caps and yellow down caps; the other final caps stay solid.
    /// </summary>
    internal static IReadOnlyDictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomLevelWord>> Cells { get; } =
        new Dictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomLevelWord>>
        {
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0x00, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x20, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x20, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x00, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0x01, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x21, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x21, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x01, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0x02, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x22, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x22, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x02, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0x03, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x23, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x23, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x03, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0x00, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x20, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x20, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x00, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0x01, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x21, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x21, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x01, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0x02, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x22, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x22, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x02, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0x03, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x23, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x23, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x03, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0x11, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x10, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x10, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x11, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0x31, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x30, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x30, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x31, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0x13, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x12, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x12, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x13, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0x33, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x32, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x32, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x33, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0x11, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x10, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x10, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x11, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0x31, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x30, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x30, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x31, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0x13, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x12, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x12, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x13, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0x33, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x32, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x32, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x33, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0x04, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x24, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x24, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x04, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0x05, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x25, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x25, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x05, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0x06, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x26, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x26, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x06, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0x07, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x27, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x27, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x07, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0x04, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x24, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x24, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x04, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0x05, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x25, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x25, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x05, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0x06, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x26, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x26, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x06, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0x07, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x27, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x27, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x07, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0x15, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x14, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x14, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x15, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0x35, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x34, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x34, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x35, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0x17, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x16, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x16, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x17, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0x37, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x36, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x36, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x37, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0x15, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x14, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x14, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x15, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0x35, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x34, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x34, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x35, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0x17, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x16, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x16, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x17, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0x37, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x36, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x36, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x37, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0x08, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x28, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x28, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x08, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0x09, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x29, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x29, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x09, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0x0a, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2a, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2a, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0a, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0x0b, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2b, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x0b, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0x08, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x28, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x28, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x08, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0x09, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x29, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x29, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x09, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0x0a, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2a, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2a, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0a, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0x0b, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2b, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2b, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x0b, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0x19, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x18, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x18, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x19, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0x39, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x38, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x38, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x39, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0x1b, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1a, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1a, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1b, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0x3b, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3a, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3a, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3b, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0x19, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x18, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x18, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x19, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0x39, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x38, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x38, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x39, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0x1b, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1a, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1a, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1b, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0x3b, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3a, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3a, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3b, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
