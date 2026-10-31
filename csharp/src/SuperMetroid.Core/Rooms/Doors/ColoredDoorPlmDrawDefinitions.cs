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
                RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(32, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(32, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(1, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(33, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(33, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(1, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(2, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(34, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(34, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(2, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(3, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(35, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(35, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(3, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(0, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(32, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(32, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(1, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(33, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(33, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(1, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(2, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(34, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(34, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(2, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(3, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(35, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(35, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(3, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(17, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(16, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(16, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(17, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(49, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(48, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(48, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(49, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(19, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(18, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(18, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(19, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(51, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(50, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(50, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(51, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(17, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(16, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(16, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(17, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(49, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(48, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(48, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(49, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(19, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(18, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(18, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(19, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(51, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(50, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(50, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(51, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(4, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(36, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(36, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(4, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(5, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(37, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(37, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(5, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(6, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(38, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(38, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(6, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(7, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(39, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(39, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(7, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(4, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(36, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(36, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(4, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(5, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(37, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(37, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(5, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(6, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(38, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(38, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(6, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(7, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(39, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(39, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(7, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(21, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(20, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(20, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(21, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(53, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(52, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(52, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(53, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(23, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(22, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(22, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(23, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(55, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(54, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(54, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(55, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(21, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(20, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(20, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(21, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(53, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(52, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(52, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(53, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(23, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(22, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(22, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(23, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(55, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(54, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(54, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(55, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(8, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(40, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(40, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(8, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(9, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(41, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(41, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(9, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(10, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(42, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(42, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(10, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(11, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(43, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(43, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(11, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(8, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(40, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(40, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(8, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(9, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(41, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(41, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(9, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(10, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(42, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(42, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(10, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(11, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(43, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(43, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(11, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(25, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(24, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(24, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(25, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(57, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(56, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(56, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(57, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(27, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(26, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(26, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(27, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(59, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(58, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(58, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(59, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(25, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(24, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(24, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(25, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(57, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(56, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(56, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(57, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(27, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(26, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(26, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(27, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(59, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(58, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(58, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(59, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
