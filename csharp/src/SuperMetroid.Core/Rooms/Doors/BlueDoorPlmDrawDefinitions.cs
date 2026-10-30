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
                RoomTerrainBlock.Of(0x0c, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x0c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(0x0c, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x0c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(0x0d, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2d, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2d, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x0d, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(0x0e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x0e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(0x0f, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2f, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x2f, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x0f, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, AliasFrame)] =
            [
                RoomTerrainBlock.Of(0x0c, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x0c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(0x0c, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x2c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(0x0c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(0x0d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x0d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(0x0e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x0e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(0x0f, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x2f, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x2f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(0x0f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, AliasFrame)] =
            [
                RoomTerrainBlock.Of(0x1d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1d, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(0x1d, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1d, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(0x3d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3c, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3c, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3d, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(0x1f, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(0x3f, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, AliasFrame)] =
            [
                RoomTerrainBlock.Of(0x1d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1d, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(0x1d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1c, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(0x1d, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(0x3d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3c, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3d, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(0x1f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x1f, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(0x3f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(0x3f, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
