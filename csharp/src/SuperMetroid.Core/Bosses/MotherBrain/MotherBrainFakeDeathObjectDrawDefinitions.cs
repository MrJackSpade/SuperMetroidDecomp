using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain fake-death room physical PLM draws: their shapes and how their cells collide. What each draw shows is
/// its installed picture (#1557).
/// </summary>
internal static class MotherBrainFakeDeathObjectDrawDefinitions
{
    /// <summary>
    /// The shapes of the two thirteen-cell solid fill rows and six tube clears. The middle pair
    /// duplicates a seven-cell column; side tubes clear one extra cell toward the center using signed origin-relative X.
    /// </summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, MotherBrainFakeDeathDraws.RegularDraw> RegularShapes = new()
    {
        [MotherBrainFakeDeathDraw.UnusedFirstSolidRow] = new(MotherBrainFakeDeathDraw.UnusedFirstSolidRow, 13, 1, MotherBrainFakeDeathClear.FillRow, 0),
        [MotherBrainFakeDeathDraw.UnusedSecondSolidRow] = new(MotherBrainFakeDeathDraw.UnusedSecondSolidRow, 13, 1, MotherBrainFakeDeathClear.FillRow, 0),
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = new(MotherBrainFakeDeathDraw.ClearCeilingBlock, 2, 1, MotherBrainFakeDeathClear.CeilingTube, 0),
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = new(MotherBrainFakeDeathDraw.ClearCeilingTube, 5, 1, MotherBrainFakeDeathClear.CeilingTube, 0),
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = new(MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube, 4, 1, MotherBrainFakeDeathClear.FloorTube, 0),
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = new(MotherBrainFakeDeathDraw.ClearBottomMiddleTubes, 7, 2, MotherBrainFakeDeathClear.FloorTube, 0),
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = new(MotherBrainFakeDeathDraw.ClearBottomLeftTube, 5, 2, MotherBrainFakeDeathClear.FloorTube, 1),
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = new(MotherBrainFakeDeathDraw.ClearBottomRightTube, 5, 2, MotherBrainFakeDeathClear.FloorTube, -1),
    };

    /// <summary>
    /// How the cells of the wall-fill, escape-door, fill-row and tube-clear draws collide, one row per run. The wall is
    /// solid; the two-column door has a door parent above three vertical extensions in its first column and air in its
    /// second. The fill rows are solid; tube interiors become air, and their ceiling and floor endpoints slopes.
    /// </summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, RoomCollisionType[][]> Collisions = new()
    {
        [MotherBrainFakeDeathDraw.FillWall] =
        [
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [RoomCollisionType.SolidBlock],
        ],
        [MotherBrainFakeDeathDraw.EscapeDoor] =
        [
            [
                RoomCollisionType.DoorBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
        ],
        [MotherBrainFakeDeathDraw.UnusedFirstSolidRow] =
        [
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        ],
        [MotherBrainFakeDeathDraw.UnusedSecondSolidRow] =
        [
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] =
        [
            [
                RoomCollisionType.Slope,
                RoomCollisionType.Air,
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearCeilingTube] =
        [
            [
                RoomCollisionType.Slope,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] =
        [
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] =
        [
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] =
        [
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ],
            [RoomCollisionType.Air],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] =
        [
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ],
            [RoomCollisionType.Air],
        ],
    };

    /// <summary>The cells of each mural row, left to right.</summary>
    internal const int BackgroundWidth = 13;

    /// <summary>The twelve mural rows, top to bottom; a row's position is its row of the mural.</summary>
    internal static readonly MotherBrainFakeDeathDraw[] BackgroundRows =
    [
        MotherBrainFakeDeathDraw.MuralRow1, MotherBrainFakeDeathDraw.MuralRow2, MotherBrainFakeDeathDraw.MuralRow3,
        MotherBrainFakeDeathDraw.MuralRow4, MotherBrainFakeDeathDraw.MuralRow5, MotherBrainFakeDeathDraw.MuralRow6,
        MotherBrainFakeDeathDraw.MuralRow7, MotherBrainFakeDeathDraw.MuralRow8, MotherBrainFakeDeathDraw.MuralRow9,
        MotherBrainFakeDeathDraw.MuralRow10, MotherBrainFakeDeathDraw.MuralRow11, MotherBrainFakeDeathDraw.MuralRow12,
    ];
}
