using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Physical bank-$84 draw layouts selected by Mother Brain's glass PLM. The
/// multi-run records include signed offsets from the PLM origin; the native
/// instruction program still owns damage thresholds, shards, and timing.
/// </summary>
internal static class MotherBrainGlassPlmDrawDefinitions
{
    /// <summary>
    /// The cells of the eleven glass states, one row per native draw run. Vertical panes reflect their lower halves;
    /// the shifted pane mirrors its inner column, and shatter states clear the outer columns before replacing the
    /// centre supports. NTSC source: 84:9717..9846.
    /// </summary>
    internal static readonly Dictionary<MotherBrainGlassDraw, RoomLevelWord[][]> Cells = new()
    {
        [MotherBrainGlassDraw.Initial] =
        [
            [RoomLevelWord.Create(0x2c0, LevelBlockFlipFlags.Horizontal, RoomCollisionType.ShootableBlock)],
        ],
        [MotherBrainGlassDraw.PaneDamage1] =
        [
            [
                RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2c9, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c9, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2c8, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c8, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.PaneDamage2] =
        [
            [
                RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2cb, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2cb, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2ca, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2ca, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.PaneTransition] =
        [
            [
                RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2cc, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cc, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane1] =
        [
            [RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock)],
            [
                RoomLevelWord.Create(0x2cd, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c9, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c9, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cd, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2c8, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c8, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane2] =
        [
            [RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock)],
            [
                RoomLevelWord.Create(0x2cb, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cb, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cd, LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2ca, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2ca, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane3] =
        [
            [RoomLevelWord.Create(0x2c7, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock)],
            [
                RoomLevelWord.Create(0x2cc, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cc, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.Shatter1] =
        [
            [
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2cf, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cf, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cf, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cf, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2d0, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2d0, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomLevelWord.Create(0x2d0, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2d0, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.Shatter2] =
        [
            [
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2c2, LevelBlockFlipFlags.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x2c3, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c3, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c4, LevelBlockFlipFlags.None, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomLevelWord.Create(0x2c2, LevelBlockFlipFlags.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x2c3, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c3, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c4, LevelBlockFlipFlags.Horizontal, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.Shatter3] =
        [
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0x2d2, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d4, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0x2d2, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d4, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.Cleared] =
        [
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ],
        ],
    };

    /// <summary>Each glass state's editable presentation frame name.</summary>
    internal static readonly Dictionary<MotherBrainGlassDraw, string> VisualIds = new()
    {
        [MotherBrainGlassDraw.Initial] = "initial",
        [MotherBrainGlassDraw.PaneDamage1] = "pane-damage-1",
        [MotherBrainGlassDraw.PaneDamage2] = "pane-damage-2",
        [MotherBrainGlassDraw.PaneTransition] = "pane-transition",
        [MotherBrainGlassDraw.ShiftedPane1] = "shifted-pane-1",
        [MotherBrainGlassDraw.ShiftedPane2] = "shifted-pane-2",
        [MotherBrainGlassDraw.ShiftedPane3] = "shifted-pane-3",
        [MotherBrainGlassDraw.Shatter1] = "shatter-1",
        [MotherBrainGlassDraw.Shatter2] = "shatter-2",
        [MotherBrainGlassDraw.Shatter3] = "shatter-3",
        [MotherBrainGlassDraw.Cleared] = "cleared",
    };
}
