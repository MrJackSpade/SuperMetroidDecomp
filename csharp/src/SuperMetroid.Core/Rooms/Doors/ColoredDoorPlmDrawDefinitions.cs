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
    internal static IReadOnlyDictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomTerrainBlock>> Cells { get; } =
        new Dictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomTerrainBlock>>
        {
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(0x00, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x20, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x20, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x00, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(0x01, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x21, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x21, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x01, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(0x02, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x22, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x22, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x02, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(0x03, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x23, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x23, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x03, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(0x00, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x20, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x20, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x00, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(0x01, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x21, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x21, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x01, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(0x02, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x22, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x22, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x02, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(0x03, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x23, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x23, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x03, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(0x11, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x10, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x10, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x11, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(0x31, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x30, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x30, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x31, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(0x13, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x12, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x12, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x13, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(0x33, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x32, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x32, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x33, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(0x11, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x10, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x10, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x11, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(0x31, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x30, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x30, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x31, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(0x13, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x12, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x12, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x13, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(0x33, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x32, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x32, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x33, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(0x04, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x24, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x24, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x04, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(0x05, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x25, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x25, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x05, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(0x06, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x26, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x26, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x06, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(0x07, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x27, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x27, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x07, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(0x04, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x24, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x24, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x04, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(0x05, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x25, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x25, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x05, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(0x06, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x26, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x26, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x06, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(0x07, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x27, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x27, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x07, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(0x15, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x14, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x14, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x15, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(0x35, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x34, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x34, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x35, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(0x17, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x16, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x16, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x17, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(0x37, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x36, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x36, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x37, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(0x15, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x14, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x14, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x15, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(0x35, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x34, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x34, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x35, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(0x17, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x16, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x16, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x17, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(0x37, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x36, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x36, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x37, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(0x08, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x28, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x28, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x08, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(0x09, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x29, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x29, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x09, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(0x0a, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2a, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2a, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x0a, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(0x0b, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2b, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x2b, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x0b, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(0x08, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x28, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x28, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x08, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(0x09, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x29, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x29, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x09, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(0x0a, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2a, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2a, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x0a, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(0x0b, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2b, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x2b, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x0b, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(0x19, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x18, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x18, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x19, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(0x39, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x38, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x38, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x39, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(0x1b, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1a, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1a, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1b, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(0x3b, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3a, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3a, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3b, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(0x19, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x18, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x18, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x19, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(0x39, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x38, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x38, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x39, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(0x1b, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1a, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1a, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1b, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(0x3b, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3a, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3a, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3b, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
