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
    internal static IReadOnlyDictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomLevelWord>> Cells { get; } =
        new Dictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomLevelWord>>
        {
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, AliasFrame)] =
            [
                RoomLevelWord.Create(0x0c, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0x0c, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2c, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0x0d, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0d, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0x0e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0x0f, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2f, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2f, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x0f, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, AliasFrame)] =
            [
                RoomLevelWord.Create(0x0c, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0x0c, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2c, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0x0d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0x0e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0x0f, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2f, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x0f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, AliasFrame)] =
            [
                RoomLevelWord.Create(0x1d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1c, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0x1d, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x1c, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0x3d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3d, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0x1f, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0x3f, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, AliasFrame)] =
            [
                RoomLevelWord.Create(0x1d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0x1d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x1c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0x3d, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3d, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0x1f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1f, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0x3f, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3f, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
