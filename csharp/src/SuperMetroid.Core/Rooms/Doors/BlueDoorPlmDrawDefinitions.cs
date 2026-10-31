using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Physical four-block level writes for the four opening frames of each blue-door cap.
/// The bank-$84 instruction programs still own sound, frame timing, and deletion.
/// </summary>
internal static class BlueDoorPlmDrawDefinitions
{
    /// <summary>The physical closed-cap alias frame, which shows frame zero's artwork with only its first cell solid.</summary>
    internal const int AliasFrame = -1;

    /// <summary>
    /// The four cells of each of the twenty blue-cap draws, in native run order. Vertical caps advance one tile per
    /// frame with a 32-tile inner row; horizontal caps alternate tileset rows and advance two tiles per frame pair.
    /// Mirrors follow the facing and the cell's half of the cap. The alias shows frame zero's tiles but makes only its
    /// first cell solid; final vertical frames clear their middle cells, and horizontal frames stay solid.
    /// </summary>
    internal static IReadOnlyDictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomTerrainBlock>> Cells { get; } =
        new Dictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomTerrainBlock>>
        {
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, AliasFrame)] =
            [
                RoomTerrainBlock.Of(12, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(44, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(44, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(12, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(12, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(44, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(44, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(12, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(13, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(45, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(45, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(13, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(14, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(46, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(46, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(14, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(15, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(47, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(47, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(15, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, AliasFrame)] =
            [
                RoomTerrainBlock.Of(12, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(44, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(44, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(12, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(12, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(44, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(44, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(12, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(13, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(45, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(45, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(13, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(14, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(46, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(46, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(14, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(15, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(47, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(47, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(15, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, AliasFrame)] =
            [
                RoomTerrainBlock.Of(29, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(28, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(28, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(29, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(29, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(28, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(28, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(29, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(61, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(60, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(60, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(61, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(31, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(30, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(30, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(31, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(63, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(62, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(62, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(63, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, AliasFrame)] =
            [
                RoomTerrainBlock.Of(29, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(28, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(28, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(29, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(29, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(28, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(28, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(29, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(61, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(60, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(60, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(61, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(31, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(30, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(30, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(31, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(63, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(62, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(62, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(63, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
