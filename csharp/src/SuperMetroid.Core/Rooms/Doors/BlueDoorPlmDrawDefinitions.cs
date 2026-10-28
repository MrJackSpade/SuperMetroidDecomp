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
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomLevelWord.Create(0x0d, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0d, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomLevelWord.Create(0x0e, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0e, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomLevelWord.Create(0x0f, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2f, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2f, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x0f, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, AliasFrame)] =
            [
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x0c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomLevelWord.Create(0x0d, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2d, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0d, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomLevelWord.Create(0x0e, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2e, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x0e, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomLevelWord.Create(0x0f, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2f, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2f, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x0f, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, AliasFrame)] =
            [
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomLevelWord.Create(0x3d, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3d, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomLevelWord.Create(0x1f, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1f, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomLevelWord.Create(0x3f, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3f, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, AliasFrame)] =
            [
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1c, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x1d, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomLevelWord.Create(0x3d, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3c, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3d, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomLevelWord.Create(0x1f, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1e, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x1f, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomLevelWord.Create(0x3f, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3e, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x3f, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
