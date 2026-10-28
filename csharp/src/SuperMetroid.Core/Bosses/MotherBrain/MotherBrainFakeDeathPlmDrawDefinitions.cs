using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain fake-death room physical PLM draws at $84:94A3..9716.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitions
{
    /// <summary>The stable visual document name of each draw in the closed bank-$84 draw domain.</summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, string> VisualIds = new()
    {
        [MotherBrainFakeDeathDraw.FillWall] = "fill-wall",
        [MotherBrainFakeDeathDraw.EscapeDoor] = "escape-door",
        [MotherBrainFakeDeathDraw.BackgroundRow2] = "background-row-2",
        [MotherBrainFakeDeathDraw.BackgroundRow3] = "background-row-3",
        [MotherBrainFakeDeathDraw.BackgroundRow4] = "background-row-4",
        [MotherBrainFakeDeathDraw.BackgroundRow5] = "background-row-5",
        [MotherBrainFakeDeathDraw.BackgroundRow6] = "background-row-6",
        [MotherBrainFakeDeathDraw.BackgroundRow7] = "background-row-7",
        [MotherBrainFakeDeathDraw.BackgroundRow8] = "background-row-8",
        [MotherBrainFakeDeathDraw.BackgroundRow9] = "background-row-9",
        [MotherBrainFakeDeathDraw.BackgroundRowA] = "background-row-a",
        [MotherBrainFakeDeathDraw.BackgroundRowB] = "background-row-b",
        [MotherBrainFakeDeathDraw.BackgroundRowC] = "background-row-c",
        [MotherBrainFakeDeathDraw.BackgroundRowD] = "background-row-d",
        [MotherBrainFakeDeathDraw.BackgroundRowEUnused] = "background-row-e-unused",
        [MotherBrainFakeDeathDraw.BackgroundRowFUnused] = "background-row-f-unused",
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = "clear-ceiling-block",
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = "clear-ceiling-tube",
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = "clear-bottom-middle-side-tube",
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = "clear-bottom-middle-tubes",
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = "clear-bottom-left-tube",
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = "clear-bottom-right-tube",
    };

    /// <summary>
    /// Native 84:966D..9716: the shapes of the two thirteen-cell solid fill rows and six tube clears. The middle pair
    /// duplicates a seven-cell column; side tubes clear one extra cell toward the center using signed origin-relative X.
    /// </summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, MotherBrainFakeDeathDraws.RegularDraw> RegularShapes = new()
    {
        [MotherBrainFakeDeathDraw.BackgroundRowEUnused] = new(MotherBrainFakeDeathDraw.BackgroundRowEUnused, 13, 1, MotherBrainFakeDeathClear.FillRow, 0),
        [MotherBrainFakeDeathDraw.BackgroundRowFUnused] = new(MotherBrainFakeDeathDraw.BackgroundRowFUnused, 13, 1, MotherBrainFakeDeathClear.FillRow, 0),
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = new(MotherBrainFakeDeathDraw.ClearCeilingBlock, 2, 1, MotherBrainFakeDeathClear.CeilingTube, 0),
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = new(MotherBrainFakeDeathDraw.ClearCeilingTube, 5, 1, MotherBrainFakeDeathClear.CeilingTube, 0),
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = new(MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube, 4, 1, MotherBrainFakeDeathClear.FloorTube, 0),
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = new(MotherBrainFakeDeathDraw.ClearBottomMiddleTubes, 7, 2, MotherBrainFakeDeathClear.FloorTube, 0),
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = new(MotherBrainFakeDeathDraw.ClearBottomLeftTube, 5, 2, MotherBrainFakeDeathClear.FloorTube, 1),
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = new(MotherBrainFakeDeathDraw.ClearBottomRightTube, 5, 2, MotherBrainFakeDeathClear.FloorTube, -1),
    };

    /// <summary>
    /// The cells of the wall-fill, escape-door, fill-row and tube-clear draws, one row per run. 84:94A3/94B1: the
    /// wall's solid caps mirror tile 30F around center tile 340; the two-column door has a door parent above three
    /// vertical extensions in its first column (broken upper edge 222/223, interior panels 1AF and 1EB, lower interior
    /// strip 1D0, broken lower edge 220/221) and air in its second. 84:966D..9716: fill rows are solid; tube interiors
    /// become blank FF, and ceiling and floor endpoints restore 12FC/1339.
    /// </summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, RoomLevelWord[][]> Cells = new()
    {
        [MotherBrainFakeDeathDraw.FillWall] =
        [
            [
                RoomLevelWord.Create(0x340, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x30f, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
            [RoomLevelWord.Create(0x30f, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock)],
        ],
        [MotherBrainFakeDeathDraw.EscapeDoor] =
        [
            [
                RoomLevelWord.Create(0x222, LevelBlockFlipFlags.None, RoomCollisionType.DoorBlock),
                RoomLevelWord.Create(0x1af, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x1d0, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x220, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomLevelWord.Create(0x223, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x1eb, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x1d0, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x221, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainFakeDeathDraw.BackgroundRowEUnused] =
        [
            [
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x319, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainFakeDeathDraw.BackgroundRowFUnused] =
        [
            [
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] =
        [
            [
                RoomLevelWord.Create(0x2fc, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearCeilingTube] =
        [
            [
                RoomLevelWord.Create(0x2fc, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] =
        [
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x339, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] =
        [
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x339, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ],
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x339, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] =
        [
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x339, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ],
            [RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air)],
        ],
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] =
        [
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x339, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ],
            [RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air)],
        ],
    };

    /// <summary>
    /// Retained picture composition, not numeric samples: 84:9505..966C places
    /// this 13x12 Tourian pipe/panel mural at room (2,2), as shown by A9:8C87..8D10.
    /// Tileset E (8F:E720) expands these cells into individually placed bends,
    /// junctions, panels and gaps. Repeated pipe segments do not determine their
    /// routing; a switch or fitted rule would merely re-encode that chosen picture.
    /// Retain under #1165's nonsense exception. Geometry and collision are calculated
    /// separately. Provenance and reconstruction are in motherBrainBackgroundArtworkReview.
    /// </summary>
    internal static readonly RoomLevelWord[][] BackgroundVisuals =
    [
        [Tile(577), Tile(578), Tile(764), Tile(764), Tile(764), Tile(579), Tile(580), Tile(764), Tile(581), Tile(578, FlipX), Tile(577), Tile(577), Tile(582)],
        [Tile(495, FlipY), Tile(434), Tile(485), Tile(485), Tile(486), Tile(485), Tile(485), Tile(485), Tile(485), Tile(434, FlipX), Tile(495, FlipY), Tile(495, FlipY), Tile(434)],
        [Tile(433), Tile(466), Tile(454), Tile(455), Tile(255), Tile(518), Tile(519), Tile(255), Tile(422), Tile(458, FlipY), Tile(524, FlipX), Tile(433, FlipX), Tile(521, FlipY)],
        [Tile(465), Tile(498), Tile(420), Tile(487), Tile(420), Tile(550), Tile(551), Tile(421), Tile(420), Tile(525), Tile(521, FlipXY), Tile(433), Tile(427)],
        [Tile(433), Tile(530), Tile(452), Tile(457), Tile(452), Tile(518), Tile(519), Tile(453), Tile(452), Tile(552, FlipX), Tile(428), Tile(492), Tile(492)],
        [Tile(433), Tile(524, FlipY), Tile(458, FlipX), Tile(455, FlipXY), Tile(426), Tile(424), Tile(424), Tile(424), Tile(424), Tile(552, FlipX), Tile(427), Tile(461), Tile(461)],
        [Tile(465), Tile(464), Tile(490, FlipX), Tile(255), Tile(255), Tile(518), Tile(519), Tile(255), Tile(423), Tile(525, FlipY), Tile(521, FlipX), Tile(491), Tile(464)],
        [Tile(491), Tile(491), Tile(490, FlipX), Tile(255), Tile(255), Tile(518), Tile(519), Tile(255), Tile(422), Tile(255), Tile(556, FlipY), Tile(521, FlipX), Tile(430)],
        [Tile(492), Tile(431), Tile(490, FlipX), Tile(455, FlipX), Tile(454, FlipX), Tile(518), Tile(519), Tile(424), Tile(422), Tile(424), Tile(424), Tile(466, FlipX), Tile(430)],
        [Tile(428), Tile(431), Tile(434), Tile(487, FlipX), Tile(485), Tile(550), Tile(551), Tile(485), Tile(422), Tile(486), Tile(485), Tile(434, FlipX), Tile(461)],
        [Tile(524, FlipX), Tile(495), Tile(434), Tile(485), Tile(486), Tile(485), Tile(485), Tile(486), Tile(485), Tile(485), Tile(485), Tile(434, FlipX), Tile(495)],
        [Tile(584), Tile(585), Tile(586), Tile(587), Tile(825), Tile(588), Tile(589), Tile(825), Tile(590), Tile(825), Tile(825), Tile(591), Tile(585)],
    ];

    /// <summary>A mirrored mural cell.</summary>
    private const LevelBlockFlipFlags FlipX = LevelBlockFlipFlags.Horizontal;
    /// <summary>A vertically flipped mural cell.</summary>
    private const LevelBlockFlipFlags FlipY = LevelBlockFlipFlags.Vertical;
    /// <summary>A mural cell flipped both ways.</summary>
    private const LevelBlockFlipFlags FlipXY = LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical;

    /// <summary>One air mural cell: tileset tile <paramref name="index"/> with <paramref name="flips"/>.</summary>
    private static RoomLevelWord Tile(int index, LevelBlockFlipFlags flips = LevelBlockFlipFlags.None) =>
        RoomLevelWord.Create(checked((ushort)index), flips, RoomCollisionType.Air);

    /// <summary>The twelve mural rows, top to bottom; a row's position is its row of the mural.</summary>
    internal static readonly MotherBrainFakeDeathDraw[] BackgroundRows =
    [
        MotherBrainFakeDeathDraw.BackgroundRow2, MotherBrainFakeDeathDraw.BackgroundRow3, MotherBrainFakeDeathDraw.BackgroundRow4,
        MotherBrainFakeDeathDraw.BackgroundRow5, MotherBrainFakeDeathDraw.BackgroundRow6, MotherBrainFakeDeathDraw.BackgroundRow7,
        MotherBrainFakeDeathDraw.BackgroundRow8, MotherBrainFakeDeathDraw.BackgroundRow9, MotherBrainFakeDeathDraw.BackgroundRowA,
        MotherBrainFakeDeathDraw.BackgroundRowB, MotherBrainFakeDeathDraw.BackgroundRowC, MotherBrainFakeDeathDraw.BackgroundRowD,
    ];
}
