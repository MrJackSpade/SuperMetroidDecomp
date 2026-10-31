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
    internal static IReadOnlyDictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomTerrainBlock>> Cells { get; } =
        new Dictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomTerrainBlock>>
        {
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, ClearFrame)] =
            [
                RoomTerrainBlock.Of(130, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(162, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(162, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(130, BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomTerrainBlock.Of(174, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(206, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(206, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(174, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomTerrainBlock.Of(175, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(207, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(207, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(175, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomTerrainBlock.Of(176, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(208, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(208, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(176, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomTerrainBlock.Of(177, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(209, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(209, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(177, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, ClearFrame)] =
            [
                RoomTerrainBlock.Of(130, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(162, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(162, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(130, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomTerrainBlock.Of(174, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(206, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(206, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(174, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomTerrainBlock.Of(175, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(207, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(207, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(175, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomTerrainBlock.Of(176, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(208, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(208, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(176, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomTerrainBlock.Of(177, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(209, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(209, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(177, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, ClearFrame)] =
            [
                RoomTerrainBlock.Of(132, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(131, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(131, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(132, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomTerrainBlock.Of(179, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(178, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(178, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(179, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomTerrainBlock.Of(211, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(210, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(210, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(211, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomTerrainBlock.Of(181, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(180, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(180, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(181, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomTerrainBlock.Of(213, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(212, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(212, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(213, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, ClearFrame)] =
            [
                RoomTerrainBlock.Of(132, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(131, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(131, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(132, BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomTerrainBlock.Of(179, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(178, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(178, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(179, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomTerrainBlock.Of(211, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(210, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(210, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(211, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomTerrainBlock.Of(181, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(180, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(180, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(181, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomTerrainBlock.Of(213, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(212, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(212, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(213, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        };
}
