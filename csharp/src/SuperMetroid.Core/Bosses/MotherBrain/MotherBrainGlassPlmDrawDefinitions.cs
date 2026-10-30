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
            [RoomLevelWord.Create(0x2c0, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock)],
        ],
        [MotherBrainGlassDraw.PaneDamage1] =
        [
            [
                RoomLevelWord.Create(0x2c7, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2c9, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c9, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c7, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2c8, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c8, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.PaneDamage2] =
        [
            [
                RoomLevelWord.Create(0x2c7, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2cb, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2cb, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c7, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2ca, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2ca, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.PaneTransition] =
        [
            [
                RoomLevelWord.Create(0x2c7, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2cc, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cc, BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane1] =
        [
            [RoomLevelWord.Create(0x2c7, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
            [
                RoomLevelWord.Create(0x2cd, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c9, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c9, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cd, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2c8, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2c8, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane2] =
        [
            [RoomLevelWord.Create(0x2c7, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
            [
                RoomLevelWord.Create(0x2cb, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cb, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cd, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2ca, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2ca, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane3] =
        [
            [RoomLevelWord.Create(0x2c7, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
            [
                RoomLevelWord.Create(0x2cc, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cc, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.Shatter1] =
        [
            [
                RoomLevelWord.Create(0x2ce, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0x2cf, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cf, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2ce, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x2cf, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2cf, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2d0, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2d0, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomLevelWord.Create(0x2d0, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2d0, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.Shatter2] =
        [
            [
                RoomLevelWord.Create(0x2ce, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomLevelWord.Create(0x2ce, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2ce, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomLevelWord.Create(0x2c2, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x2c3, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c3, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c4, BlockMirroring.None, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomLevelWord.Create(0x2c2, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomLevelWord.Create(0x2c3, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c3, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomLevelWord.Create(0x2c4, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.Shatter3] =
        [
            [
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0x2d2, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d4, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0x2d2, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d3, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomLevelWord.Create(0x2d4, BlockMirroring.Horizontal, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.Cleared] =
        [
            [
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air),
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
