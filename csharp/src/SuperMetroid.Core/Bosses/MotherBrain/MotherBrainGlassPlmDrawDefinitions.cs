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
    internal static readonly Dictionary<MotherBrainGlassDraw, RoomTerrainBlock[][]> Cells = new()
    {
        [MotherBrainGlassDraw.Initial] =
        [
            [RoomTerrainBlock.Of(704, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock)],
        ],
        [MotherBrainGlassDraw.PaneDamage1] =
        [
            [
                RoomTerrainBlock.Of(711, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(713, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(713, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(711, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomTerrainBlock.Of(712, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(712, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.PaneDamage2] =
        [
            [
                RoomTerrainBlock.Of(711, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(715, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(715, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(711, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomTerrainBlock.Of(714, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(714, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.PaneTransition] =
        [
            [
                RoomTerrainBlock.Of(711, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(716, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(716, BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane1] =
        [
            [RoomTerrainBlock.Of(711, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
            [
                RoomTerrainBlock.Of(717, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(713, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(713, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(717, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomTerrainBlock.Of(712, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(712, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane2] =
        [
            [RoomTerrainBlock.Of(711, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
            [
                RoomTerrainBlock.Of(715, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(715, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(717, BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomTerrainBlock.Of(714, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(714, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane3] =
        [
            [RoomTerrainBlock.Of(711, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
            [
                RoomTerrainBlock.Of(716, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(716, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.Shatter1] =
        [
            [
                RoomTerrainBlock.Of(718, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(719, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(719, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(718, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomTerrainBlock.Of(718, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(719, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(719, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(718, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomTerrainBlock.Of(720, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(720, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomTerrainBlock.Of(720, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(720, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.Shatter2] =
        [
            [
                RoomTerrainBlock.Of(718, BlockMirroring.None, RoomCollisionType.ShootableBlock),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(718, BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
            ],
            [
                RoomTerrainBlock.Of(718, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(718, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            ],
            [
                RoomTerrainBlock.Of(706, BlockMirroring.None, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(707, BlockMirroring.None, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(707, BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(708, BlockMirroring.None, RoomCollisionType.VerticalExtension),
            ],
            [
                RoomTerrainBlock.Of(706, BlockMirroring.Horizontal, RoomCollisionType.HorizontalExtension),
                RoomTerrainBlock.Of(707, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(707, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.VerticalExtension),
                RoomTerrainBlock.Of(708, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
            ],
        ],
        [MotherBrainGlassDraw.Shatter3] =
        [
            [
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomTerrainBlock.Of(722, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(723, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(723, BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(724, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomTerrainBlock.Of(722, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(723, BlockMirroring.Horizontal, RoomCollisionType.Air),
                RoomTerrainBlock.Of(723, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air),
                RoomTerrainBlock.Of(724, BlockMirroring.Horizontal, RoomCollisionType.Air),
            ],
        ],
        [MotherBrainGlassDraw.Cleared] =
        [
            [
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
            ],
            [
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
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
